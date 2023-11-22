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

namespace ScoreRankForTdmx.Patches
{
    internal class SongSelectScoreRankpatch
    {
        private static readonly Dictionary<EnsoData.EnsoLevelType, Sprite> LevelIcons = new();

        [HarmonyPatch(typeof(SongSelectManager))]
        [HarmonyPatch("Start")]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void Start_Postfix(SongSelectManager __instance)
        {
            LevelIcons.Clear();

            for (var i = 0; i < 5; i++)
            {
                LevelIcons.Add((EnsoData.EnsoLevelType)i, __instance.songFilterSetting.difficultyIconSprites[i]);
            }
        }

        [HarmonyPatch(typeof(SongSelectKanban))]
        [HarmonyPatch("UpdateDisplay")]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void UpdateDisplay_Postfix(SongSelectKanban __instance, in SongSelectManager.Song song)
        {
            bool isSelectedSong = __instance.name == "Kanban1";

            var currentSong = song;
            MusicDataInterface.MusicInfoAccesser musicInfoAccesser = TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.musicInfoAccessers.Find((MusicDataInterface.MusicInfoAccesser info) => info.Id == currentSong.Id);

            int highestNormal = 0;
            int highestReached = 0;
            List<ScoreRank> ranks = new List<ScoreRank>();
            for (int i = 0; i < 5; i++)
            {
                ScoreRank scoreRank = ScoreRankPatch.GetScoreRank(song.HighScores[i].hiScoreRecordInfos.score, musicInfoAccesser.Scores[i]);
                ranks.Add(scoreRank);
                if (scoreRank != ScoreRank.None)
                {
                    highestReached = i;
                    if (i < 4)
                    {
                        highestNormal = i;
                    }
                }
            }

            if (isSelectedSong)
            {
                CreateLeftScoreRankIcon(__instance, ranks[highestNormal], isSelectedSong, false, highestNormal);
                CreateLeftScoreRankIcon(__instance, ranks[4], isSelectedSong, true, 4);
            }
            else
            {
                CreateLeftScoreRankIcon(__instance, ranks[highestReached], isSelectedSong, false, highestReached);
            }

            if (isSelectedSong)
            {
                // 
                for (int i = 0; i < 5; i++)
                {
                    var contents = AssetUtility.GetChildByName(__instance.gameObject, "Contents");
                    var diffCourse = AssetUtility.GetChildByName(contents, "DiffCourse");
                    var diffCourseObj = AssetUtility.GetChildByName(diffCourse, "DiffCourse" + (i + 1));
                    if (diffCourseObj != null)
                    {
                        var scoreRankObj = AssetUtility.GetChildByName(diffCourseObj, "ScoreRank1P");
                        if (scoreRankObj == null)
                        {
                            scoreRankObj = AssetUtility.CreateImageChild(diffCourseObj, "ScoreRank1P", new Vector2(71, 65), Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Small", ranks[i].ToString() + ".png"));
                        }

                        var image = scoreRankObj.GetOrAddComponent<Image>();
                        image.sprite = AssetUtility.LoadSprite(Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Small", ranks[i].ToString() + ".png"));

                        scoreRankObj.transform.localScale = new Vector3(1, 1, 1);

                    }
                }
            }

        }

        public static void CreateLeftScoreRankIcon(SongSelectKanban __instance, ScoreRank scoreRank, bool isSelected, bool isUra, int difficulty)
        {
            var scoreRankObj = AssetUtility.GetChildByName(__instance.gameObject, isUra ? "UraScoreRank" : "ScoreRank");
            if (scoreRankObj == null)
            {
                scoreRankObj = AssetUtility.CreateImageChild(__instance.gameObject, isUra ? "UraScoreRank" : "ScoreRank", new Vector2(0, 0), Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Small", "WhiteIki.png"));
            }
            var image = scoreRankObj.GetOrAddComponent<Image>();
            var diffIconObj = AssetUtility.GetChildByName(scoreRankObj, "DiffIcon");
            if (diffIconObj == null)
            {
                diffIconObj = AssetUtility.CreateImageChild(scoreRankObj, "DiffIcon", new Vector2(3.3f, -15), LevelIcons[(EnsoData.EnsoLevelType)difficulty]);
            }
            if (isSelected)
            {
                diffIconObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            }
            else
            {
                diffIconObj.transform.localPosition = new Vector3(14, -5, 0);
                diffIconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            var diffIconImage = diffIconObj.GetOrAddComponent<Image>();

            if (scoreRank == ScoreRank.None)
            {
                image.color = new Color(1, 1, 1, 0);
                diffIconImage.color = new Color(1, 1, 1, 0);
            }
            else
            {
                image.sprite = AssetUtility.LoadSprite(Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Small", scoreRank.ToString() + ".png"));
                image.color = new Color(1, 1, 1, 1);
                diffIconImage.sprite = LevelIcons[(EnsoData.EnsoLevelType)difficulty];
                diffIconImage.color = new Color(1, 1, 1, 1);
            }



            if (isSelected)
            {
                if (isUra)
                {
                    scoreRankObj.transform.localPosition = new Vector2(-465, -96);
                }
                else
                {
                    scoreRankObj.transform.localPosition = new Vector2(-465, -25);
                }
                scoreRankObj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            }
            else
            {
                if (isUra)
                {
                    image.color = new Color(1, 1, 1, 0);
                    diffIconImage.color = new Color(1, 1, 1, 0);
                }
                else
                {
                    scoreRankObj.transform.localPosition = new Vector2(-410, -40);
                    scoreRankObj.transform.localScale = new Vector3(1, 1, 1);
                }
            }

        }
    }
}
