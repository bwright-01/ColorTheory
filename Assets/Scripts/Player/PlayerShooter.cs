using UnityEngine;

using Core;
using Audio.Sound;

namespace Player {

    [RequireComponent(typeof(PlayerController))]
    public class PlayerShooter : MonoBehaviour {
        [SerializeField] Weapon.Gun gun;
        [SerializeField] Weapon.Melee melee;

        [Space]
        [Space]

        [SerializeField] SingleSound noAmmoClick;

        [Space]
        [Space]

        [SerializeField][Range(0f, 10f)] float timeRecharge = 3f;
        [SerializeField][Range(0f, 10)] int numberOfShots = 5;

        [Space]
        [Space]

        [SerializeField][Range(0f, 10f)] float timeIsShootingThreshold = 0.1f;
        [SerializeField][Range(0f, 10f)] float timeIsMeleeingThreshold = 0.1f;

        [Space]
        [Space]

        [SerializeField] EventChannelSO eventChannel;

        // cached
        PlayerController controller;

        // state
        Timer recharging = new Timer();
        Timer meleeing = new Timer();
        Timer shooting = new Timer();

        public bool IsMeleeing => meleeing.active;
        public bool IsShooting => shooting.active;

        public int GetNumShots() {
            return numberOfShots;
        }

        public int GetNumAvailableShots() {
            return (int)Mathf.Floor(recharging.value * numberOfShots);
        }

        void OnEnable() {
            controller.OnFirePress.Subscribe(OnFirePress);
            controller.OnMeleePress.Subscribe(OnMeleePress);
            eventChannel.OnAbilityUpgraded.Subscribe(OnAbilityUpgraded);
        }

        void OnDisable() {
            controller.OnFirePress.Unsubscribe(OnFirePress);
            controller.OnMeleePress.Unsubscribe(OnMeleePress);
            eventChannel.OnAbilityUpgraded.Unsubscribe(OnAbilityUpgraded);
        }

        void Awake() {
            controller = GetComponent<PlayerController>();
            if (gun == null) Debug.LogError($"Gun is null in {Utils.FullGameObjectName(gameObject)}");
            if (melee == null) Debug.LogError($"Melee is null in {Utils.FullGameObjectName(gameObject)}");
            noAmmoClick.Init(this);
            recharging.SetDuration(timeRecharge);
            recharging.Start();
            shooting.SetDuration(timeIsShootingThreshold);
            meleeing.SetDuration(timeIsMeleeingThreshold);
        }

        void Start() {
            CheckForUpgrades();
        }

        void Update() {
            recharging.TickReversed();
            shooting.Tick();
            meleeing.Tick();
        }

        void OnAbilityUpgraded(Game.UpgradeType upgradeType) {
            CheckForUpgrades();
        }

        void CheckForUpgrades() {
            if (Game.GameSystems.current.state.IsWeaponUpgraded) {
                numberOfShots = Game.GameSystems.current.state.UpgradedShotsCount;
            }
            if (Game.GameSystems.current.state.IsMeleeUpgraded) {
                melee.SetDamageMultiplier(Game.GameSystems.current.state.UpgradedMeleeDamageMod);
            }
        }

        void OnFirePress() {
            if (shooting.active) return;
            if (HasAvailableBullet()) {
                shooting.Start();
                gun.TryAttack();
                SpendAmmo();
            } else {
                noAmmoClick.Play(this);
            }
        }

        void OnMeleePress() {
            if (meleeing.active) return;
            if (melee.TryAttack()) {
                meleeing.Start();
            }
        }

        bool HasAvailableBullet() {
            return GetNumAvailableShots() >= 1;
        }

        void SpendAmmo() {
            recharging.SetValue(recharging.value - (1f / numberOfShots));
        }
    }
}
