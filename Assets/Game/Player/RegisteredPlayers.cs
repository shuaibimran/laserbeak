using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTEasings;
using DTObjectPoolManager;
using InControl;

namespace DT.Game.Players {
	public static class RegisteredPlayers {
		// PRAGMA MARK - Static Public Interface
		public static event Action<Player> OnPlayerAdded = delegate {};
		public static event Action<Player> OnPlayerRemoved = delegate {};

		public static string GetDefaultNicknameFor(Player player) {
			return string.Format("P{0}", player.Index() + 1);
		}

		public static void BeginPlayerRegistration() {
			if (InGameConstants.RegisterHumanPlayers) {
				foreach (InputDevice inputDevice in InputManager.Devices) {
					RegisterPlayerFor(inputDevice);
				}

				if (InputManager.Devices.Count <= 0) {
					RegisterKeyboardPlayer();
				}
			}

			InputManager.OnDeviceAttached += HandleDeviceAttached;
			InputManager.OnDeviceDetached += HandleDeviceDetached;
		}

		public static void FinishPlayerRegistration() {
			InputManager.OnDeviceAttached -= HandleDeviceAttached;
			InputManager.OnDeviceDetached -= HandleDeviceDetached;
		}

		public static bool IsInputDeviceAlreadyRegistered(InputDevice inputDevice) {
			// null inputDevice represents AI player
			if (inputDevice == null) {
				return false;
			}

			return players_.Any(p => p.Input.GetInputDevice() == inputDevice);
		}

		public static void Add(Player player) {
			if (IsInputDeviceAlreadyRegistered(player.Input.GetInputDevice())) {
				Debug.LogWarning("Cannot add player: " + player + " because input device: " + player.Input.GetInputDevice() + " is already registered!");
				return;
			}

			players_.Add(player);
			OnPlayerAdded.Invoke(player);
		}

		public static void Remove(Player player) {
			players_.Remove(player);
			OnPlayerRemoved.Invoke(player);
		}

		public static void Clear() {
			Player[] removedPlayers = players_.ToArray();

			players_.Clear();

			foreach (Player player in removedPlayers) {
				OnPlayerRemoved.Invoke(player);
			}
		}

		public static IList<Player> AllPlayers {
			get { return players_; }
		}

		public static IEnumerable<IInputWrapper> AllInputs {
			get { return players_.Where(p => p.Input != null).Select(p => p.Input); }
		}


		// PRAGMA MARK - Static Internal
		private static readonly List<Player> players_ = new List<Player>();

		private static void RegisterPlayerFor(InputDevice inputDevice) {
			if (IsInputDeviceAlreadyRegistered(inputDevice)) {
				return;
			}

			Player player = new Player(new InputWrapperDevice(inputDevice));
			player.Nickname = "";
			player.Skin = null;

			Add(player);
		}

		private static void RegisterKeyboardPlayer() {
			Player player = new Player(new InputWrapperKeyboard());
			player.Nickname = "";
			player.Skin = null;

			Add(player);
		}

		private static void HandleDeviceAttached(InputDevice inputDevice) {
			// TODO (darren): only allow these handlers during specific times (player customization?)
			// right now players can join in middle of game.. which is bleh
			RegisterPlayerFor(inputDevice);
		}

		private static void HandleDeviceDetached(InputDevice inputDevice) {
			var player = players_.FirstOrDefault(p => p.Input.GetInputDevice() == inputDevice);
			if (player == null) {
				Debug.LogWarning("DeviceRemoved but not found in player list, unexpected.");
				return;
			}

			Remove(player);
		}
	}
}