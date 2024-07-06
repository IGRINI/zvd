using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TabItem : MonoBehaviour
{
   public const string SELECTED_COLOR = "SelectedColor";
   public const string UNSELECTED_COLOR = "UnselectedColor";
   
   public TabsView.TabInfo TabInfo => _tabInfo;
   public Toggle Toggle => _tabToggle;
   
   [SerializeField] private TMP_Text _tabNameText;
   [SerializeField] private Toggle _tabToggle;

   private Color _selectedColor;
   private Color _unselectedColor;
   private Action _onTabSelect;
   private TabsView.TabInfo _tabInfo;

   [Inject]
   private void Constructor(TabsView.TabInfo tabInfo, ToggleGroup toggleGroup,
      [Inject(Optional = true, Id = SELECTED_COLOR)] Color selectedColorText = default,
      [Inject(Optional = true, Id = UNSELECTED_COLOR)] Color unselectedColorText = default)
   {
      _tabInfo = tabInfo;
      _selectedColor = selectedColorText == default ? Color.white : selectedColorText;
      _unselectedColor = unselectedColorText == default ? Color.white : unselectedColorText;
      _tabNameText.SetText(tabInfo.TabName);
      _tabToggle.group = toggleGroup;
      _onTabSelect = tabInfo.OnTabSelect;
   }

   private void Awake()
   {
      _tabToggle.OnValueChangedAsObservable().Subscribe(value =>
      {
         _tabNameText.color = value ? _selectedColor : _unselectedColor;
         
         if(value)
            _onTabSelect?.Invoke();
      });
   }
}
