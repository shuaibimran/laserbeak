using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DT.Game.Battle.Lasers;
using DTAnimatorStateMachine;
using DTEasings;
using DTObjectPoolManager;
using InControl;

namespace DT.Game.Battle.Players {
	public class BattlePlayerSkinOutlet : BattlePlayerComponent, IRecycleSetupSubscriber, IRecycleCleanupSubscriber {
		// PRAGMA MARK - IRecycleSetupSubscriber Implementation
		public void OnRecycleSetup() {
			Player_.OnSkinChanged += HandleSkinChanged;
		}


		// PRAGMA MARK - IRecycleCleanupSubscriber Implementation
		public void OnRecycleCleanup() {
			Player_.OnSkinChanged -= HandleSkinChanged;
		}


		// PRAGMA MARK - Internal
		[SerializeField]
		private Renderer[] bodyRenderers_;

		private void HandleSkinChanged() {
			foreach (var renderer in bodyRenderers_) {
				renderer.material.SetColor("_DiffuseColor", Player_.Skin.BodyColor);
			}
		}
	}
}