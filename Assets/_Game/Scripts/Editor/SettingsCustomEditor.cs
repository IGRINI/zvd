using System;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Installers;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SettingsInstaller))]
    public class SettingsInstallerEditor : OdinEditor
    {
        private SettingsInstaller settingsInstaller;
        private NetworkInfoController.Settings networkSettings;
        private string[] teamNames;
        private int teamCount;

        private void OnEnable()
        {
            settingsInstaller = (SettingsInstaller)target;
            networkSettings = settingsInstaller.UnitsSettings;
            teamNames = Enum.GetNames(typeof(Team));
            teamCount = teamNames.Length;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Team Relations Matrix", EditorStyles.boldLabel);

            // Draw column headers
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(100));
            for (int i = 0; i < teamCount; i++)
            {
                EditorGUILayout.LabelField(teamNames[i], GUILayout.Width(100));
            }

            EditorGUILayout.EndHorizontal();

            // Draw rows
            for (int i = 0; i < teamCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(teamNames[i], GUILayout.Width(100));

                for (int j = 0; j < teamCount; j++)
                {
                    if (i == j)
                    {
                        EditorGUILayout.LabelField("-", GUILayout.Width(100));
                        continue;
                    }

                    var relation = GetRelation((Team)i,
                        (Team)j);
                    if (GUILayout.Button(relation.ToString(), GUILayout.Width(100)))
                    {
                        SetNextRelation((Team)i, (Team)j);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private RelationType GetRelation(Team teamA,
            Team teamB)
        {
            foreach (var relation in networkSettings.TeamRelations)
            {
                if ((relation.TeamA == teamA && relation.TeamB == teamB) ||
                    (relation.TeamA == teamB && relation.TeamB == teamA))
                {
                    return relation.Relation;
                }
            }

            return RelationType.Neutral;
        }

        private void SetNextRelation(Team teamA,
            Team teamB)
        {
            for (int i = 0; i < networkSettings.TeamRelations.Count; i++)
            {
                if ((networkSettings.TeamRelations[i].TeamA == teamA &&
                     networkSettings.TeamRelations[i].TeamB == teamB) ||
                    (networkSettings.TeamRelations[i].TeamA == teamB &&
                     networkSettings.TeamRelations[i].TeamB == teamA))
                {
                    networkSettings.TeamRelations[i].Relation =
                        (RelationType)(((int)networkSettings.TeamRelations[i].Relation +
                                        1) % Enum.GetValues(
                            typeof(RelationType)).Length);
                    return;
                }
            }

            networkSettings.TeamRelations.Add(new NetworkInfoController.Settings.TeamRelation
            {
                TeamA = teamA,
                TeamB = teamB,
                Relation = RelationType.Hostile
            });
        }
    }
}