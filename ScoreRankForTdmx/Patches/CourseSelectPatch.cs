using HarmonyLib;
using SongSelect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static MusicDataInterface;
using static SongSelectManager;

namespace ScoreRankForTdmx.Patches
{
    internal class CourseSelectPatch
    {
        static List<GameObject> CourseButtonObjects = new List<GameObject>(5);

        static MusicDataInterface.MusicInfoAccesser currentSong;

        [HarmonyPatch(typeof(CourseSelect))]
        [HarmonyPatch(nameof(CourseSelect.SetInfo))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void CourseSelect_SetInfo_Postfix(CourseSelect __instance, MusicDataInterface.MusicInfoAccesser song)
        {
            currentSong = song;
        }


        [HarmonyPatch(typeof(CourseSelect))]
        [HarmonyPatch(nameof(CourseSelect.UpdateDiffCourseAnim))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void CourseSelect_UpdateDiffCourseAnim_Postfix(CourseSelect __instance)
        {
            try
            {
                GetCourseButtonObjects();
                for (int i = 0; i < 5; i++)
                {
                    EnsoRecordInfo ensoRecordInfo;
                    __instance.playDataManager.GetPlayerRecordInfo(0, currentSong.UniqueId, (EnsoData.EnsoLevelType)i, out ensoRecordInfo);
                    ScoreRank scoreRank = ScoreRankPatch.GetScoreRank(ensoRecordInfo.normalHiScore.score, currentSong.Scores[i]);

                    var imageObj = AssetUtility.GetChildByName(CourseButtonObjects[i], "ScoreRank1P");
                    if (imageObj == null)
                    {
                        imageObj = AssetUtility.CreateImageChild(CourseButtonObjects[i], "ScoreRank1P", new Rect(88, 350, 40, 40), Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Small", scoreRank.ToString() + ".png"));
                    }
                    else
                    {
                        var image = imageObj.GetOrAddComponent<Image>();
                        image.sprite = AssetUtility.LoadSprite(Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Small", scoreRank.ToString() + ".png"));
                    }
                    if (scoreRank == ScoreRank.None)
                    {
                        imageObj.transform.localScale = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        imageObj.transform.localScale = new Vector3(1, 1, 1);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.LogError(e.Message);
            }
        }

        static void GetCourseButtonObjects()
        {
            CourseButtonObjects = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                CourseButtonObjects.Add(null);
            }
            var courseSelectObject = GameObject.Find("CourseSelect");
            if (courseSelectObject != null)
            {
                var songSelectCourse = AssetUtility.GetChildByName(courseSelectObject, "SongSelectCourse");
                if (songSelectCourse != null)
                {
                    var kanban = AssetUtility.GetChildByName(songSelectCourse, "Kanban");
                    if (kanban != null)
                    {
                        var diffcourse = AssetUtility.GetChildByName(kanban, "DiffCourse");
                        if (diffcourse != null)
                        {
                            for (int i = 1; i < 6; i++)
                            {
                                var btnDiffCourse = AssetUtility.GetChildByName(diffcourse, "BtnDiffCourse" + i);
                                if (btnDiffCourse != null)
                                {
                                    CourseButtonObjects[i - 1] = AssetUtility.GetChildByName(btnDiffCourse, "DiffCourse");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
