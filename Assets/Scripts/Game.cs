using Assets.PROMETEO___Car_Controller.Scripts;
using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameEntity
{
    Undef = 0,
    Free,
    Traffic,
    Player
}

public class Track<T>
{
    public Track(T[][] track)
    {
        _track = track;
    }

    public int Width => _track.Length;
    public int Length => _track[0].Length;

    public T GetAt(int x, int y) => _track[x][y];
    public void SetAt(int x, int y, T value) => _track[x][y] = value;

    public T GetAt(Vector2Int pos) => _track[pos.x][pos.y];
    public T SetAt(Vector2Int pos, T value) => _track[pos.x][pos.y] = value;

    private readonly T[][] _track;
}

public class Game : SystemBase
{
    public string ExclusiveShowPlayerName { get; set; } = string.Empty;

    public bool SkipIntro { get => _initialDelay < Mathf.Epsilon; set => _initialDelay = 0f; }

    public float InitialCountDown { get => _initialDelay; }

    public int Seed { get; set; }

    public string Port { get; set; }

    public float Tick { get => _tickDt; }

    public int TrackLength { get => _trackLength; }
    
    public int TrackWidth { get => _playerNames.Length * 2 + 1; }

    public int StepId { get; private set; } = -1;

    public TrackModifier TrackModifier { get; private set; }

    public GameEntity GetAt(Vector2Int pos) => _track.GetAt(pos);

    public GameEntity GetAt(int x, int y) => _track.GetAt(x, y);

    public Player GetPlayer(Vector2Int pos)
    {
        if (GetAt(pos) == GameEntity.Player)
        {
            return _players.Find((p) => p.Pos == pos);
        }

        return null;
    }

    public int PlayerCount => _playerNames.Length;

    public void SetPlayerNames(string[] names)
    {
        _playerNames = names;
    }

    public Player GetPlayer(int x, int y)
    {
        if (GetAt(x, y) == GameEntity.Player)
        {
            return _players.Find((p) => p.Pos.x == x && p.Pos.y == y);
        }

        return null;
    }

    protected override void InitInternal()
    {
        TrackInitializer initializer = new TrackInitializer(Seed, _trafficProbability / 100.0);
        _track = initializer.CreateTrack(_playerNames.Length * 2 + 1, _trackLength);

        int i = 0;
        for (int x = 0; x < _track.Width; x++)
        {
            if (_track.GetAt(x, 0) == GameEntity.Player)
            {
                _players.Add(CreatePlayer(new Vector2Int(x, 0), _playerNames[i], i));
                i++;
            }
        }

        TrackModifier = new TrackModifier(_track);
        TrackModifier.OnBeforeCarMoved += HandleCarMoved;
        TrackModifier.OnPlayerLosed += HandlePlayerLosed;
        TrackModifier.OnPlayerFinished += HandlePlayerFinished;
        _snapshotMaker = new TrackSnapshotMaker(_track, _snapshotObstacleSearchRadius);
    }

    private void OnDestroy()
    {
        TrackModifier.OnBeforeCarMoved -= HandleCarMoved;
        TrackModifier.OnPlayerLosed -= HandlePlayerLosed;
        TrackModifier.OnPlayerFinished -= HandlePlayerFinished;
    }

    private Player CreatePlayer(Vector2Int pos, string name, int idx)
    {
        GameObject playerGameObject = Instantiate(_playerSample, transform);
        Player player = playerGameObject.GetComponent<Player>();
        player.Pos = pos;
        player.Init(name, idx);
        PlayerClient client = playerGameObject.GetComponent<PlayerClient>();
        client.Port = Port;
        return player;
    }

    protected override void UpdateInternal()
    {
        if (_initialDelay > 0f)
        {
            _initialDelay -= Time.deltaTime;
            return;
        }

        _delay -= Time.deltaTime;
        if (_delay > .0f)
        {
            return;
        }

        _delay += _tickDt;
        MakeStep();
    }

    private void MakeStep()
    {
        StepId++;

        foreach (Player player in _players)
        {
            TrackModifier.QueueMove(new MoveRequest() { From = player.Pos, Side = player.GetMove() });
        }
        TrackModifier.ApplyMove();

        foreach (Player player in _players)
        {
            TrackSnapshot snapshot = _snapshotMaker.GetSnapshot(player, _players);
            player.RequestMove(snapshot);
        }

        if (TrackModifier.FinishOverride > 0)
        {
            TrackModifier.FinishOverride--;
        }
    }

    private void HandleCarMoved(Move move)
    {
        if (_track.GetAt(move.From) == GameEntity.Player)
        {
            GetPlayer(move.From).Pos = move.To;
        }
    }

    private void HandlePlayerFinished(int x, int y)
    {
        Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Player finished but there is no player on track");
        Debug.Log($"Finished at [{x}, {y}]");
        Player player = GetPlayer(x, y);
        _players.Remove(GetPlayer(x, y));
        _scoreController.NotifyPlayerFinished(player);
        if (TrackModifier.FinishOverride < 0)
        {
            TrackModifier.FinishOverride = _track.Length + 1;
        }
        Destroy(player.gameObject);
    }

    private void HandlePlayerLosed(int x, int y)
    {
        Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Player losed but there is no player on track");
        Player player = GetPlayer(x, y);
        _players.Remove(GetPlayer(x, y));
        _scoreController.NotifyPlayerLosed(player);
        Destroy(player.gameObject);
    }

    [SerializeField] private int _trackLength = 100;
    [SerializeField] [Range(0, 100)] private int _trafficProbability = 50;
    [SerializeField] private float _tickDt = .5f;
    [SerializeField] ConfigManager _configManager;
    [SerializeField] private Vector2Int _snapshotObstacleSearchRadius;
    [SerializeField] private GameObject _playerSample;
    [SerializeField] private ScoreController _scoreController;
    [SerializeField] private float _initialDelay = 10f;

    private Track<GameEntity> _track;
    private TrackSnapshotMaker _snapshotMaker;
    private readonly List<Player> _players = new List<Player>();
    private float _delay = .0f;
    private string[] _playerNames;
}