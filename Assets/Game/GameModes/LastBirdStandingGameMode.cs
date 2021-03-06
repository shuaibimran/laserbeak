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
	[CreateAssetMenu(fileName = "LastBirdStandingGameMode", menuName = "Game/Modes/LastBirdStandingGameMode")]
	public class LastBirdStandingGameMode : GameMode {
		// PRAGMA MARK - Public Interface
		public override string DisplayTitle {
			get { return "LAST BIRD STANDING"; }
		}

		public override int Id {
			get { return GameMode.GetIdFor<LastBirdStandingGameMode>(); }
		}


		// PRAGMA MARK - Internal
		protected override void Activate() {
			PlayerSpawner.SpawnAllPlayers();

			List<GameModeIntroView.Icon> icons = new List<GameModeIntroView.Icon>();
			foreach (Player player in RegisteredPlayers.AllPlayers) {
				icons.Add(GameModeIntroView.Icon.Player);
				icons.Add(GameModeIntroView.Icon.Swords);
			}
			icons.RemoveLast();

			GameModeIntroView.Show(DisplayTitle, icons);

			PlayerSpawner.OnSpawnedPlayerRemoved += HandleSpawnedPlayerRemoved;
		}

		protected override void CleanupInternal() {
			PlayerSpawner.OnSpawnedPlayerRemoved -= HandleSpawnedPlayerRemoved;
		}

		private void HandleSpawnedPlayerRemoved() {
			if (PlayerSpawner.AllSpawnedPlayers.Count() > 1) {
				return;
			}

			Finish();
			foreach (Player player in PlayerSpawner.AllSpawnedPlayers) {
				PlayerScores.IncrementPendingScoreFor(player);
			}
		}
	}
}