
using UnityEngine;

using Core;

namespace Actor {

    public class Health : MonoBehaviour {
        [SerializeField] float startingHP = 100f;
        [SerializeField] bool isInvulnerable = false;
        [SerializeField] float timeInvincibleAfterHit = 0f;

        [HideInInspector] public HealthEventHandler OnHealthGained = new HealthEventHandler();
        [HideInInspector] public HealthEventHandler OnDamageTaken = new HealthEventHandler();
        [HideInInspector] public HealthEventHandler OnDeath = new HealthEventHandler();

        // public
        public float Hp => hp;
        public float healthPercentage => Mathf.Clamp01(hp / startingHP);

        // cached
        Collider2D[] colliders;

        // state
        bool isAlive = true;
        float hp = 100f;
        Timer timeInvincible = new Timer();

        public Collider2D[] GetColliders() {
            return colliders;
        }

        void Awake() {
            colliders = GetComponentsInChildren<Collider2D>(true);
            hp = startingHP;
        }

        public bool IsAlive() {
            return hp > 0 && isAlive;
        }

        public void SetIsInvulnerable(bool value) {
            isInvulnerable = value;
        }

        public bool GainHealth(float amount) {
            if (!isAlive) return false;
            hp += amount;
            hp = Mathf.Min(startingHP, hp);
            OnHealthGained.Invoke(amount, hp);
            return true;
        }

        public bool TakeDamage(float damage) {
            if (!isAlive) return false;
            if (damage <= 0) return false;
            if (isInvulnerable && damage != Constants.INSTAKILL) return false;
            if (timeInvincible.active && damage != Constants.INSTAKILL) return false;
            if (hp <= 0) return false;

            hp -= damage;

            if (hp <= 0) {
                isAlive = false;
                OnDeath.Invoke(damage, hp);
                DisableColliders();
            } else {
                OnDamageTaken.Invoke(damage, hp);
            }

            timeInvincible.SetDuration(timeInvincibleAfterHit);
            timeInvincible.Start();

            return true;
        }

        void Update() {
            timeInvincible.Tick();
        }

        void DisableColliders() {
            if (colliders == null) return;
            foreach (var collider in colliders) {
                if (collider != null) collider.enabled = false;
            }
        }
    }
}
