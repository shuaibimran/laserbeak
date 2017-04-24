using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle;
using DT.Game.Battle.Players;
using DT.Game.Players;
using DT.Game.Scoring;

namespace DT.Game.GameModes {
	[CreateAssetMenu(fileName = "TeamBattleGameMode", menuName = "Game/Modes/TeamBattleGameMode")]
	public class TeamBattleGameMode : GameMode {
		// PRAGMA MARK - Public Interface
		public override void Cleanup() {
			PlayerSpawner.OnSpawnedPlayerRemoved -= HandleSpawnedPlayerRemoved;
		}


		// PRAGMA MARK - Internal
		[Header("Outlets")]
		[SerializeField]
		private GameObject[] accessoryPrefabs_;

		[SerializeField]
		private int numberOfTeams_ = 2;

		private HashSet<Player>[] teams_;

		protected override void Activate() {
			ArenaManager.Instance.LoadRandomArena();
			PlayerSpawner.SpawnAllPlayers();

			List<Player> players = RegisteredPlayers.AllPlayers.ToList();
			players.Shuffle();

			int playersPerTeam = Mathf.CeilToInt(RegisteredPlayers.AllPlayers.Count / numberOfTeams_);
			teams_ = new HashSet<Player>[numberOfTeams_];

			for (int i = 0; i < players.Count; i++) {
				int teamIndex = (int)(i / playersPerTeam);
				if (teams_[teamIndex] == null) {
					teams_[teamIndex] = new HashSet<Player>();
				}

				teams_[teamIndex].Add(players[i]);
			}

			// set override color for players
			for (int i = 0; i < teams_.Length; i++) {
				HashSet<Player> team = teams_[i];

				foreach (Player player in team) {
					BattlePlayer battlePlayer = PlayerSpawner.GetBattlePlayerFor(player);
					ObjectPoolManager.Create(accessoryPrefabs_[i], parent: battlePlayer.AccessoriesContainer);
				}
			}

			foreach (HashSet<Player> team in teams_) {
				BattlePlayerTeams.DeclareTeam(team.Select(p => PlayerSpawner.GetBattlePlayerFor(p)));
			}

			List<int> playerOrdering = new List<int>();
			List<GameModeIntroView.Icon> icons = new List<GameModeIntroView.Icon>();
			for (int i = 0; i < teams_.Length - 1; i++) {
				playerOrdering.AddRange(teams_[i].Select(p => RegisteredPlayers.AllPlayers.IndexOf(p)));
				icons.AddRange(teams_[i].Select(p => GameModeIntroView.Icon.Player));
				icons.Add(GameModeIntroView.Icon.Swords);
			}
			playerOrdering.AddRange(teams_[teams_.Length - 1].Select(p => RegisteredPlayers.AllPlayers.IndexOf(p)));
			icons.AddRange(teams_[teams_.Length - 1].Select(p => GameModeIntroView.Icon.Player));

			GameModeIntroView.Show("ANGELS VS DEMONS", icons, playerOrdering);

			PlayerSpawner.OnSpawnedPlayerRemoved += HandleSpawnedPlayerRemoved;
		}

		private void HandleSpawnedPlayerRemoved() {
			int? teamIndexLeft = null;
			for (int i = 0; i < teams_.Length; i++) {
				HashSet<Player> team  = teams_[i];
				bool teamAlive = team.Any(p => PlayerSpawner.IsAlive(p));
				if (teamAlive) {
					// if multiple teams still alive
					if (teamIndexLeft != null) {
						return;
					}

					teamIndexLeft = i;
				}
			}

			Finish();
			// possible that no teams are left alive
			if (teamIndexLeft != null) {
				foreach (Player player in teams_[(int)teamIndexLeft].Where(p => PlayerSpawner.IsAlive(p))) {
					PlayerScores.IncrementPendingScoreFor(player);
				}
			}
		}
	}
}