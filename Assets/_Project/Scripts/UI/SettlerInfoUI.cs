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
        private TMP_Text _nameText;
        private TMP_Text _roleText;
        private TMP_Text _buildingText;
        private TMP_Text _taskText;
        private TMP_Text _statusText;
        private bool _built;

        private SettlerWalker _current;
        private float _refreshTimer;
        private const float RefreshInterval = 0.3f;

        public SettlerWalker Current => _current;

        public void ShowInfo(SettlerWalker walker)
        {
            if (!_built) BuildUI();
            _current = walker;
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

            if (!_current.IsActive || _current.gameObject == null)
            {
                HideInfo();
                return;
            }

            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= RefreshInterval)
            {
                _refreshTimer = 0f;
                RefreshDisplay();
            }
        }

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            foreach (Transform child in root)
                Destroy(child.gameObject);

            _nameText = UIPrimitiveFactory.AddThemedText(root, "Settler", 20,
                UIColors.Default.Gold, TextAlignmentOptions.Center, UIStyleType.HeaderText);
            _nameText.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;

            var divider = new GameObject("Divider", typeof(RectTransform));
            divider.transform.SetParent(root, false);
            divider.AddComponent<LayoutElement>().preferredHeight = 2;
            var divImg = divider.AddComponent<Image>();
            divImg.color = UIColors.Default.Muted;
            divImg.raycastTarget = false;

            _roleText = UIPrimitiveFactory.AddThemedText(root, "", 15,
                UIColors.Default.Text, TextAlignmentOptions.MidlineLeft, UIStyleType.BodyText);
            _roleText.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            _buildingText = UIPrimitiveFactory.AddThemedText(root, "", 15,
                UIColors.Default.Text, TextAlignmentOptions.MidlineLeft, UIStyleType.BodyText);
            _buildingText.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            _taskText = UIPrimitiveFactory.AddThemedText(root, "", 15,
                UIColors.Default.Text, TextAlignmentOptions.MidlineLeft, UIStyleType.BodyText);
            _taskText.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            _statusText = UIPrimitiveFactory.AddThemedText(root, "", 14,
                UIColors.Default.Muted, TextAlignmentOptions.MidlineLeft, UIStyleType.BodyText);
            _statusText.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            _built = true;
        }

        private void RefreshDisplay()
        {
            if (_current == null) return;

            string roleName = _current.Role != SettlerRole.None
                ? SettlerRoleInfo.GetDisplayName(_current.Role)
                : "Unassigned";

            var roleColor = _current.Role != SettlerRole.None ? UIColors.Default.Ok : UIColors.Default.Warn;

            if (_nameText != null)
                _nameText.text = roleName;

            if (_roleText != null)
            {
                _roleText.text = $"Role: <color=#{ColorUtility.ToHtmlStringRGB(roleColor)}>{roleName}</color>";
            }

            if (_buildingText != null)
            {
                string bldName = _current.AssignedBuilding != null
                    ? _current.AssignedBuilding.Data.DisplayName
                    : "None";
                _buildingText.text = $"Workplace: {bldName}";
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
                _taskText.text = $"Status: <color=#{ColorUtility.ToHtmlStringRGB(taskColor)}>{taskDesc}</color>";
            }

            if (_statusText != null)
            {
                string detail = "";
                if (_current.AssignedBuilding != null)
                {
                    int assigned = 0;
                    if (SettlerJobManager.Instance != null)
                        assigned = SettlerJobManager.Instance.GetAssignedWorkerCount(_current.AssignedBuilding);
                    int required = _current.AssignedBuilding.Data.GetTotalRequiredWorkers();
                    detail = $"Workers at site: {assigned}/{required}";
                }
                _statusText.text = detail;
            }
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
