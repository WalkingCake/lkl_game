using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVisualizer : SystemBase
{
    protected override void InitInternal()
    {
        CreateCars();

        InitPlane();

        _game.TrackModifier.OnCarDisapeared += HandleCarDisapeared;
        _game.TrackModifier.OnBeforeCarMoved += HandleCarMoved;
    }

    private void OnDestroy()
    {
        _game.TrackModifier.OnCarDisapeared -= HandleCarDisapeared;
        _game.TrackModifier.OnBeforeCarMoved -= HandleCarMoved;
    }

    private void CreateCars()
    {
        System.Random rand = new System.Random(_game.Seed);

        CarMover[][] movers = new CarMover[_game.TrackWidth][];
        for (int i = 0; i < movers.Length; i++)
        {
            movers[i] = new CarMover[_game.TrackLength];
        }
        _carMovers = new Track<CarMover>(movers);


        foreach (Player player in GetComponentsInChildren<Player>())
        {
            CarMover carMover = player.GetComponent<CarMover>();
            carMover.Tick = _game.Tick;
            _carMovers.SetAt(player.Pos, carMover);
            player.transform.position = ToWorld(player.Pos);
        }

        for (int x = 0; x < _game.TrackWidth; x++)
        {
            for (int y = 0; y < _game.TrackLength; y++)
            {
                if (_game.GetAt(x, y) == GameEntity.Traffic)
                {
                    int carIdx = rand.Next(_trafficSample.Count);
                    GameObject traffic = GameObject.Instantiate(_trafficSample[carIdx]);
                    CarMover mover = traffic.GetComponent<CarMover>();
                    mover.Tick = _game.Tick;
                    _carMovers.SetAt(x, y, mover);
                    traffic.transform.position = this.transform.position + new Vector3(x * _cellSize.x, .0f, y * _cellSize.y);
                }
            }
        }

    }
    
    private void InitPlane()
    {
        float left = -_borderOffset.x;
        float right = _cellSize.x * _game.TrackWidth + _borderOffset.x;
        float down = -_borderOffset.y;
        float up = _cellSize.y * _game.TrackLength + _borderOffset.y;

        Mesh mesh = new Mesh()
        {
            vertices = new Vector3[4] {
            new Vector3(left, .0f, down),
            new Vector3(left, .0f, up),
            new Vector3(right, .0f, up),
            new Vector3(right, .0f, down)
            },
            triangles = new int[6] { 0, 1, 2, 2, 3, 0 }
        };

        _road.mesh = mesh;

        Vector3 buildingVel = _cellSize.y / _game.Tick * (-Vector3.forward);
        _spawner1.Init(new Vector3( left / 2f, _road.transform.position.y, up), buildingVel, up - down);
        _spawner2.Init(new Vector3(right + left / 2f, _road.transform.position.y, up), buildingVel, up - down);
    }

    private void HandleCarMoved(Move move)
    {
        if (_carMovers.GetAt(move.From) == null)
        {
            Debug.Assert(false, $"Move {move} is incorrect");
        } 

        _carMovers.GetAt(move.From).Target = ToWorld(move.To);
        _carMovers.SetAt(move.To, _carMovers.GetAt(move.From));
        _carMovers.SetAt(move.From, null);
    }

    private void HandleCarDisapeared(int x, int y)
    {
        GameObject.Destroy(_carMovers.GetAt(x, y).gameObject);
    }

    private Vector3 ToWorld(Vector2Int pos)
    {
        return new Vector3(pos.x * _cellSize.x, transform.position.y, pos.y * _cellSize.y);
    }

    [SerializeField] private Game _game;
    [SerializeField] private Vector2 _cellSize;
    [SerializeField] private Vector2 _borderOffset;
    [SerializeField] private List<GameObject> _trafficSample;
    [SerializeField] private MeshFilter _road;
    [SerializeField] private BuildingSpawner _spawner1;
    [SerializeField] private BuildingSpawner _spawner2;

    private Track<CarMover> _carMovers;
}
