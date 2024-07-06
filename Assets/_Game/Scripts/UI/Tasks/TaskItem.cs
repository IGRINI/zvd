using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Tasks
{
    public class TaskItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _taskNameText;
        [SerializeField] private TMP_Text _counterText;
        [SerializeField] private Image _fillImage;
        [SerializeField] private RectTransform _fillPointerTransform;

        private TaskData _taskData;
        private float _fillSize;

        private void Awake()
        {
            //TEST INIT
            Initialize(new TaskData("TEST TASK", Random.Range (5, 30)));
            //END TEST INIT

            //TEST SET PROGRESS
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(_ =>
            {
                SetProgress(Random.Range(0, (_taskData.TargetCount + 1)));
            }).AddTo(this);
            //END TEST SET PROGRESS
        }

        private void Start()
        {
            Observable.NextFrame().Subscribe(_ =>
            {
                _fillSize = _fillImage.rectTransform.rect.width;
            }).AddTo(this);
        }

        public void Initialize(TaskData data)
        {
            _taskData = data;
            _taskNameText.SetText(data.TaskName);
            _counterText.SetText($"0/{data.TargetCount}");
        }

        public void SetProgress(int currentCount, bool animate = true)
        {
            var percent = Mathf.Clamp((float)currentCount / _taskData.TargetCount, 0, 1f);
            _counterText.SetText($"{currentCount}/{_taskData.TargetCount}");
            DOTween.Sequence()
                .Append(_fillImage.DOFillAmount(percent, animate ? .2f : 0f))
                .Join(_fillPointerTransform.DOAnchorPosX(percent * _fillSize,
                    animate ? .2f : 0f));
        }
    }

    public struct TaskData
    {
        public string TaskName { get; private set; }
        public int TargetCount { get; private set; }

        public TaskData(string taskName, int targetCount)
        {
            TaskName = taskName;
            TargetCount = targetCount;
        }
    }
}

