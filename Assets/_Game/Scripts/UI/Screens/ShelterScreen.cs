using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Utils.Screens
{
    public class ShelterScreen : Screen
    {
        [SerializeField] private TabsView _topTabsView;
        [SerializeField] private GameObject _charsViewObject;
        [SerializeField] private GameObject _stashViewObject;
        [SerializeField] private GameObject _abilitiesViewObject;

        private List<TabsView.TabInfo> _tabs = new();

        protected override void Awake()
        {
            base.Awake();

            _tabs.Add(new ("Characters", delegate { SetTabView(0); }));
            _tabs.Add(new ("Stash",delegate { SetTabView(1); }));
            _tabs.Add(new ("Abilities",delegate { SetTabView(2); }));
            
            _topTabsView.Initialize(_tabs);
        }

        public void SetTabView(int index)
        {
            _topTabsView.SetActiveTabWithoutNotify(_tabs[index]);
            _stashViewObject.SetActive(index == 1);
        }
    }
}
