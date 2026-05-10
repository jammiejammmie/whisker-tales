using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Detox
{
    public sealed class DetoxMomentService : MonoBehaviour
    {
        [SerializeField] private DetoxMessageRepository repository;
        [SerializeField] private int levelsBetweenChecks = 3;
        [SerializeField] private float probability = 0.33f;

        private int completedSinceLastCheck;

        private void OnEnable()
        {
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void HandleLevelCompleted(int level, int stars)
        {
            completedSinceLastCheck++;

            if (completedSinceLastCheck < levelsBetweenChecks)
            {
                return;
            }

            completedSinceLastCheck = 0;

            if (Random.value <= probability)
            {
                GameEvents.RaiseDetoxModalShown();
                DebugLogger.Info(LogCategory.UI, "Detox modal requested after level " + level);
            }
        }

        public string GetMessage()
        {
            if (repository == null)
            {
                return "잠깐 쉬어가도 괜찮아요.";
            }

            return repository.GetRandomMessage();
        }
    }
}
