using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Domain.Walkers;
using HollowGround.NPCs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class SettlerInfoUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image _roleDot;
        [SerializeField] private TMP_Text _roleTitleText;

        [Header("Identity")]
        [SerializeField] private TMP_Text _roleValueText;
        [SerializeField] private TMP_Text _buildingText;
        [SerializeField] private TMP_Text _taskText;
        [SerializeField] private TMP_Text _workersText;

        [Header("Morale")]
        [SerializeField] private Image _moraleFill;
        [SerializeField] private TMP_Text _moraleStatusText;

        [Header("Health")]
        [SerializeField] private Image _healthFill;
        [SerializeField] private TMP_Text _healthStatusText;
        [SerializeField] private TMP_Text _hospitalText;

        [Header("Production")]
        [SerializeField] private TMP_Text _efficiencyText;
        [SerializeField] private TMP_Text _specialistText;

        [Header("Actions")]
        [SerializeField] private Button _reassignBtn;
        [SerializeField] private Button _restBtn;
        [SerializeField] private Button _dismissBtn;

        private SettlerWalker _current;
        private float _refreshTimer;
        private const float RefreshInterval = 0.3f;

        public SettlerWalker Current => _current;

        private void Awake()
        {
            if (_reassignBtn != null) _reassignBtn.onClick.AddListener(OnReassignClicked);
            if (_restBtn != null) _restBtn.onClick.AddListener(OnRestClicked);
            if (_dismissBtn != null) _dismissBtn.onClick.AddListener(OnDismissClicked);
        }

        public void ShowInfo(SettlerWalker walker)
        {
            _current = walker;
            _refreshTimer = RefreshInterval;
            gameObject.SetActive(true);
            RefreshDisplay();
            SmartPosition();
        }

        public void HideInfo()
        {
            _current = null;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _current = null;
        }

        private void Update()
        {
            if (_current == null)
            {
                HideInfo();
                return;
            }

            if (!_current.gameObject.activeInHierarchy)
            {
                HideInfo();
                return;
            }

            if (!_current.IsActive)
            {
                HideInfo();
                return;
            }

            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= RefreshInterval)
            {
                _refreshTimer = 0f;
                RefreshDisplay();
                SmartPosition();
            }
        }

        private void RefreshDisplay()
        {
            if (_current == null) return;

            string roleName = _current.Role != SettlerRole.None
                ? SettlerRoleInfo.GetDisplayName(_current.Role)
                : "Idle";

            var roleColor = _current.Role != SettlerRole.None ? UIColors.Default.Ok : UIColors.Default.Warn;

            if (_roleTitleText != null) _roleTitleText.text = roleName.ToUpper();
            if (_roleDot != null) _roleDot.color = roleColor;

            if (_roleValueText != null)
            {
                _roleValueText.text = roleName;
                _roleValueText.color = roleColor;
            }

            if (_buildingText != null)
            {
                _buildingText.text = _current.AssignedBuilding != null
                    ? _current.AssignedBuilding.Data.DisplayName
                    : "None";
            }

            if (_taskText != null)
            {
                string taskDesc = _current.CurrentTask switch
                {
                    WalkerState.WalkingToTarget => "Walking to work",
                    WalkerState.WaitingAtTarget => "Working",
                    WalkerState.ReturningHome => "Returning home",
                    WalkerState.Resting => "Resting",
                    _ => "Idle"
                };
                var taskColor = _current.CurrentTask == WalkerState.WaitingAtTarget ? UIColors.Default.Ok :
                    _current.CurrentTask == WalkerState.Resting ? UIColors.Default.Muted :
                    UIColors.Default.Text;
                _taskText.text = taskDesc;
                _taskText.color = taskColor;
            }

            if (_workersText != null)
            {
                if (_current.AssignedBuilding != null && SettlerJobManager.Instance != null)
                {
                    int assigned = SettlerJobManager.Instance.GetAssignedWorkerCount(_current.AssignedBuilding);
                    int required = _current.AssignedBuilding.Data.GetTotalRequiredWorkers();
                    _workersText.text = $"{assigned}/{required}";
                }
                else
                {
                    _workersText.text = "-";
                }
            }

            if (_moraleFill != null)
            {
                _moraleFill.fillAmount = 0.75f;
                _moraleFill.color = Color.Lerp(UIColors.Default.Danger, UIColors.Default.Ok, 0.75f);
            }
            if (_moraleStatusText != null) _moraleStatusText.text = "Normal";

            if (_healthFill != null)
            {
                _healthFill.fillAmount = 1.0f;
                _healthFill.color = UIColors.Default.Ok;
            }
            if (_healthStatusText != null) _healthStatusText.text = "Healthy";

            if (_efficiencyText != null) _efficiencyText.text = "100%";
            if (_specialistText != null) _specialistText.text = "None";

            UpdateActionButtonStates();
        }

        private void UpdateActionButtonStates()
        {
            if (_reassignBtn != null)
                _reassignBtn.interactable = _current.HasJob;

            if (_restBtn != null)
                _restBtn.interactable = _current.HasJob && _current.CurrentTask != WalkerState.Resting;
        }

        private void OnReassignClicked()
        {
            if (_current == null) return;

            var walker = _current;
            string prevRole = SettlerRoleInfo.GetDisplayName(walker.Role);
            HideInfo();

            if (SettlerJobManager.Instance != null)
                SettlerJobManager.Instance.ReleaseAndReassign(walker);

            ToastUI.Show($"{prevRole} reassigned", UIColors.Default.Ok);
        }

        private void OnRestClicked()
        {
            if (_current == null) return;

            var walker = _current;
            walker.ForceRest();
            HideInfo();

            ToastUI.Show("Settler resting", UIColors.Default.Muted);
        }

        private void OnDismissClicked()
        {
            if (_current == null) return;

            var walker = _current;
            HideInfo();

            if (SettlerManager.Instance != null)
                SettlerManager.Instance.RemoveSettler(walker);

            ToastUI.Show("Settler dismissed", UIColors.Default.Warn);
        }

        private void SmartPosition()
        {
            if (_current == null) return;

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            var rt = GetComponent<RectTransform>();
            if (rt == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var canvasRt = canvas.GetComponent<RectTransform>();
            float canvasW = canvasRt.rect.width;
            float canvasH = canvasRt.rect.height;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            float panelW = rt.rect.width;
            float panelH = rt.rect.height;
            float halfPW = panelW * 0.5f;
            float halfPH = panelH * 0.5f;
            float spacing = 20f;

            Vector3 settlerPos = _current.transform.position + Vector3.up * 1f;
            Vector2 screenPos = cam.WorldToScreenPoint(settlerPos);

            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPos, null, out canvasPos);

            float safeLeft = -canvasW * 0.5f + spacing;
            float safeRight = canvasW * 0.5f - spacing;
            float safeTop = canvasH * 0.5f - 50f - spacing;
            float safeBottom = -canvasH * 0.5f + 70f + spacing;

            float targetX = canvasPos.x + halfPW + spacing;
            float targetY = canvasPos.y;

            if (targetX + halfPW > safeRight)
                targetX = canvasPos.x - halfPW - spacing;

            if (targetX - halfPW < safeLeft)
                targetX = canvasPos.x;

            targetY = Mathf.Clamp(targetY, safeBottom + halfPH, safeTop - halfPH);
            targetX = Mathf.Clamp(targetX, safeLeft + halfPW, safeRight - halfPW);

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(targetX, targetY);
        }
    }
}
