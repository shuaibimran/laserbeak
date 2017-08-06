using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle;
using DT.Game.Battle.Players;
using DT.Game.GameModes;
using DT.Game.Players;

namespace DT.Game.Scoring {
	public class ScoringState : DTStateMachineBehaviour<GameStateMachine> {
		// PRAGMA MARK - Internal
		private const float kShowDelay = 1.7f;
		private const float kResetCameraDelay = 0.7f;

		protected override void OnStateEntered() {
			if (!PlayerScores.HasPendingScores) {
				HandleScoringFinished();
				return;
			}

			InGameConstants.AllowChargingLasers = false;
			InGameConstants.EnableQuacking = true;
			BattleCamera.Instance.SetSurvivingPlayersAsTransformsOfInterest();

			CoroutineWrapper.DoAfterDelay(kShowDelay, () => {
				InGamePlayerScoringView.Show(HandleScoringFinished);
			});
		}

		protected override void OnStateExited() {
			// cleanup battle here
			BattlePlayerTeams.ClearTeams();
			BattleRecyclables.Clear();
			PlayerSpawner.CleanupAllPlayers();
			AISpawner.CleanupAllPlayers();

			InGameConstants.AllowChargingLasers = true;
			InGameConstants.EnableQuacking = false;
		}

		private void HandleScoringFinished() {
			if (PlayerScores.HasWinner) {
				StateMachine_.HandleGameFinished();
			} else {
				BattleCamera.Instance.ClearTransformsOfInterest();
				BattlePlayerPart.RemoveCollidersFromAll();
				CoroutineWrapper.DoAfterDelay(kResetCameraDelay, () => {
					StateMachine_.Continue();
				});
			}
		}
	}
}