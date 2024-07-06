using System;
using System.Collections.Generic;
using Game.PrefabsActions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TabsView : MonoBehaviour
{
    [Inject] private PrefabCreator _prefabCreator;

    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private RectTransform _tabsParent;
    [SerializeField] private TabItem _tabPrefab;
    [SerializeField] private GameObject _tabSeparatorPrefab;

    private List<TabItem> _tabItems = new();
    private List<GameObject> _separators = new();
    private CompositeDisposable _disposable = new();

    public void SetActiveTabWithoutNotify(TabInfo tab)
    {
        for (var i = 0; i < _tabItems.Count; i++)
        {
            _tabItems[i].Toggle.SetIsOnWithoutNotify(_tabItems[i].TabInfo == tab);
        }
    }
    
    public void Initialize(List<TabInfo> tabs)
    {
        _disposable.Clear();
        for (var i = 0; i < _tabItems.Count; i++)
        {
            Destroy(_tabItems[i].gameObject);
        }
        _tabItems.Clear();

        for (var i = 0; i < _separators.Count; i++)
        {
            Destroy(_separators[i]);
        }
        _separators.Clear();

        for (var i = 0; i < tabs.Count; i++)
        {
            var item = _prefabCreator.Create<TabItem>(_tabPrefab, _tabsParent, container =>
            {
                container.BindInstance(tabs[i]);
                container.BindInstance(_toggleGroup);
                container.BindInstance(Color.white).WithId(TabItem.SELECTED_COLOR);
                container.BindInstance(new Color(1f, 1f, 1f, .5f)).WithId(TabItem.UNSELECTED_COLOR);
            });
            
            if ((i + 1) < tabs.Count)
                _separators.Add(_prefabCreator.Create(_tabSeparatorPrefab, _tabsParent));

            _tabItems.Add(item);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tabsParent);
    }

    public class TabInfo
    {
        public string TabName;
        public Action OnTabSelect;

        public TabInfo(string tabName, Action onTabSelect)
        {
            TabName = tabName;
            OnTabSelect = onTabSelect;
        }
    }
}
