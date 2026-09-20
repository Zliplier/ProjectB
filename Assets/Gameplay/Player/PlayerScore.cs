using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Player
{
    public class PlayerScore : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField] private float score = 0f;
        
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Header("Events")]
        public UnityEvent<float> onScoreChanged;
        
        public float ScorePoints
        {
            get => score;
            set
            {
                score = Mathf.Max(0, value);
                onScoreChanged?.Invoke(score);
            }
        }

        public void Start()
        {
            if (scoreText == null)
                return;
            
            scoreText.SetText("Score: " + ScorePoints.ToString("F0"));
            onScoreChanged.AddListener((float points) =>
            {
                scoreText.SetText("Score: " + points.ToString("F0"));
            });
        }

        public void AddScore(float amount)
        {
            ScorePoints += amount;
        }
    }
}