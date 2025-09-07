using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class ConfigManager : SystemBase
    {
        protected override void InitInternal()
        {
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            if (GetCommandCount(commandLineArgs) == 0 || !TryParse(commandLineArgs))
            {
                TryParse(_dummyCommandLineArgs);
            }

            foreach (ConfigCommandRequiredBase command in _commands.Values.Where(c => c is ConfigCommandRequiredBase).Cast<ConfigCommandRequiredBase>())
            {
                command.ResolveRequired(_game);
            }
        }

        private int GetCommandCount(string[] args)
        {
            return args.Where(s => s.StartsWith("--")).Count();
        }

        private bool TryParse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (_commands.TryGetValue(args[i], out ConfigCommandBase command))
                {
                    if (i >= args.Length - 1)
                    {
                        return command.Resolve(_game);
                    }

                    int startArgIdx = ++i;
                    int endArgIdx = startArgIdx;
                    for (; i < args.Length; i++)
                    {
                        if (args[i].StartsWith("--"))
                        {
                            
                            i--;
                            break;
                        }

                        endArgIdx++;
                    }

                    if (!command.Resolve(_game, args.Skip(startArgIdx).Take(endArgIdx - startArgIdx).ToArray()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Dictionary<string, ConfigCommandBase> _commands = new Dictionary<string, ConfigCommandBase>
        {
            { "--seed", new ConfigCommandSeed() },
            { "--players", new ConfigCommandPlayers() },
            { "--port", new ConfigCommandPort() },
            { "--skip-intro", new ConfigCommandSkipIntro() },
            { "--show-exclusive", new ConfigCommandShowExclusive() },
            { "--traffic-probability", new ConfigCommandTrafficProbability() }
        };

        [SerializeField] private Game _game;
        [SerializeField] private string[] _dummyCommandLineArgs;
    }

    internal abstract class ConfigCommandBase
    {

        public bool Resolve(Game game, params string[] args)
        {
            if (_isExecuted)
            {
                return true;
            }

            if (args.Length < MinArgsCount || args.Length > MaxArgsCount)
            {
                return false;
            }

            _isExecuted = ResolveInternal(game, args);
            return _isExecuted;
        }

        protected abstract int MinArgsCount { get; }
        protected abstract int MaxArgsCount { get; }


        protected abstract bool ResolveInternal(Game game, params string[] args);

        protected bool _isExecuted = false;
    }

    internal abstract class ConfigCommandRequiredBase : ConfigCommandBase
    {
        public void ResolveRequired(Game game)
        {
            if (_isExecuted)
            {
                return;
            }
            _isExecuted = true;

            ResolveRequiredInternal(game);
        }
        protected abstract void ResolveRequiredInternal(Game game);
    }

    internal class ConfigCommandSeed : ConfigCommandRequiredBase
    {
        protected override int MinArgsCount => 1;
        protected override int MaxArgsCount => 1;

        protected override bool ResolveInternal(Game game, params string[] args)
        {
            if (int.TryParse(args[0], out int seed))
            {
                game.Seed = seed;
                return true;
            }

            return false;
        }

        protected override void ResolveRequiredInternal(Game game)
        {
            game.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
    }

    internal class ConfigCommandPlayers : ConfigCommandRequiredBase
    {
        protected override int MinArgsCount => 1;
        protected override int MaxArgsCount => int.MaxValue;

        protected override void ResolveRequiredInternal(Game game)
        {
            game.SetPlayerNames(new string[] { "InvalidPlayer1", "InvalidPlayer2", "InvalidPlayer3", "InvalidPlayer4" });
        }

        protected override bool ResolveInternal(Game game, params string[] args)
        {
            game.SetPlayerNames(args);
            return true;
        }
    }

    internal class ConfigCommandPort : ConfigCommandBase
    {
        protected override int MinArgsCount => 1;

        protected override int MaxArgsCount => 1;

        protected override bool ResolveInternal(Game game, params string[] args)
        {
            if (int.TryParse(args[0], out int _))
            {
                game.Port = args[0];
                return true;
            }

            return false;
        }
    }

    internal class ConfigCommandSkipIntro : ConfigCommandBase
    {
        protected override int MinArgsCount => 0;

        protected override int MaxArgsCount => 0;

        protected override bool ResolveInternal(Game game, params string[] args)
        {
            game.SkipIntro = true;
            return true;
        }
    }

    internal class ConfigCommandShowExclusive : ConfigCommandBase
    {
        protected override int MinArgsCount => 1;

        protected override int MaxArgsCount => 1;

        protected override bool ResolveInternal(Game game, params string[] args)
        {
            game.ExclusiveShowPlayerName = args[0];
            return true;
        }
    }

    internal class ConfigCommandTrafficProbability : ConfigCommandRequiredBase
    {
        protected override int MinArgsCount => 1;

        protected override int MaxArgsCount => 1;

        protected override bool ResolveInternal(Game game, params string[] args)
        {
            if (int.TryParse(args[0], out int trafficProbability) && trafficProbability >= 0 && trafficProbability <= 100)
            {
                game.TrafficProbability = trafficProbability;
                return true;
            }

            return false;
        }

        protected override void ResolveRequiredInternal(Game game)
        {
            game.TrafficProbability = 20;
        }
    }
}
