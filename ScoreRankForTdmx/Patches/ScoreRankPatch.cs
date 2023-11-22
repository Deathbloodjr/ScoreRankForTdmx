using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ScoreRankForTdmx.Patches
{
    enum ScoreRank
    {
        None,
        WhiteIki,
        BronzeIki,
        SilverIki,
        GoldMiyabi,
        PinkMiyabi,
        PurpleMiyabi,
        Kiwami
    }

    internal class ScoreRankPatch
    {
        static int currentScoreP1 = 0;
        static int p1ScoreRank = 0;
        static ScoreRank currentP1Rank = ScoreRank.None;

        static int currentScoreP2 = 0;
        static int p2ScoreRank = 0;
        static ScoreRank currentP2Rank = ScoreRank.None;


        [HarmonyPatch(typeof(CourseSelect))]
        [HarmonyPatch(nameof(CourseSelect.EnsoConfigSubmit))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void CourseSelect_EnsoConfigSubmit_Postfix(CourseSelect __instance)
        {
            List<MusicDataInterface.MusicInfoAccesser> musicInfoAccessers = TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.musicInfoAccessers;
            var musicInfo = musicInfoAccessers.Find((x) => x.Id == __instance.selectedSongInfo.Id);
            p1ScoreRank = musicInfo.Scores[__instance.selectedCourse];
            p2ScoreRank = musicInfo.Scores[__instance.selectedCourse2P];
            ResetScore();
        }


        [HarmonyPatch(typeof(ScorePlayer))]
        [HarmonyPatch(nameof(ScorePlayer.SetAddScorePool))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static bool ScorePlayer_SetAddScorePool_Prefix(ScorePlayer __instance, int score)
        {
            // I super fucked this up, fix it later
            if (__instance.playerNo == 0)
            {
                currentScoreP1 += score;




                var newRank = GetScoreRank(currentScoreP1, p1ScoreRank);
                if (newRank != currentP1Rank)
                {
                    currentP1Rank = newRank;
                    Plugin.LogInfo("P1ScoreRank: " + currentP1Rank);
                    // This failed, probably due to no parent
                    CreateEnsoScoreRankIcon(currentP1Rank, 0);
                }
            }
            else
            {
                currentScoreP2 += score;
                //Plugin.LogInfo("currentScoreP2: " + currentScoreP2);

                var newRank = GetScoreRank(currentScoreP2, p2ScoreRank);
                if (newRank != currentP2Rank)
                {
                    currentP2Rank = newRank;
                    Plugin.LogInfo("P2ScoreRank: " + currentP2Rank);
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(EnsoPauseMenu))]
        [HarmonyPatch(nameof(EnsoPauseMenu.OnRestartClicked))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void EnsoPauseMenu_OnRestartClicked_Prefix(EnsoPauseMenu __instance)
        {
            ResetScore();
        }

        [HarmonyPatch(typeof(EnsoPauseMenu))]
        [HarmonyPatch(nameof(EnsoPauseMenu.OnReturnClicked))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void EnsoPauseMenu_OnReturnClicked_Postfix(EnsoPauseMenu __instance)
        {
            ResetScore();
        }

        [HarmonyPatch(typeof(EnsoPauseMenu))]
        [HarmonyPatch(nameof(EnsoPauseMenu.OnButtonModeClicked))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void EnsoPauseMenu_OnButtonModeClicked_Postfix(EnsoPauseMenu __instance)
        {
            ResetScore();
        }

        [HarmonyPatch(typeof(ResultPlayer))]
        [HarmonyPatch(nameof(ResultPlayer.DisplayCrown))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void ResultPlayer_DisplayCrown_Postfix(ResultPlayer __instance)
        {
            var parent = GameObject.Find("BaseMain");

            Vector2 DesiredPosition = new Vector2(-267, 0);
            Vector2 RealPosition = new Vector2(DesiredPosition.x + 868, DesiredPosition.y + 224);

            var imageObj = AssetUtility.CreateImageChild(parent, "ScoreRankResult", RealPosition, Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Big", currentP1Rank.ToString() + ".png"));
            var image = imageObj.GetComponent<Image>();
            var imageColor = image.color;
            imageColor.a = 0;
            image.color = imageColor;
            Plugin.Instance.StartCoroutine(AssetUtility.ChangeTransparencyOverSeconds(imageObj, 1, true));

            //DesiredPosition = new Vector2(-442, 190);
            //RealPosition = new Vector2(DesiredPosition.x + 868, DesiredPosition.y + 224);

            //var imageObj2 = AssetUtility.CreateImageChild(parent, "ScoreRankResult", RealPosition, Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Big", currentP1Rank.ToString() + ".png"));
            //var image2 = imageObj2.GetComponent<Image>();
            //var imageColor2 = image2.color;
            //imageColor2.a = 0;
            //image2.color = imageColor2;

            //Plugin.Instance.StartCoroutine(AssetUtility.ChangeTransparencyOverSeconds(imageObj2, 1, true));
        }


        public static void ResetScore()
        {
            currentScoreP1 = 0;
            currentScoreP2 = 0;
            currentP1Rank = ScoreRank.None;
            currentP2Rank = ScoreRank.None;
        }

        public static ScoreRank GetScoreRank(int score, int maxScore)
        {
            var ratio = (float)score / (float)maxScore;
            if (ratio >= 1f)
            {
                return ScoreRank.Kiwami;
            }
            else if (ratio >= 0.95f)
            {
                return ScoreRank.PurpleMiyabi;
            }
            else if (ratio >= 0.9f)
            {
                return ScoreRank.PinkMiyabi;
            }
            else if (ratio >= 0.8f)
            {
                return ScoreRank.GoldMiyabi;
            }
            else if (ratio >= 0.7f)
            {
                return ScoreRank.SilverIki;
            }
            else if (ratio >= 0.6f)
            {
                return ScoreRank.BronzeIki;
            }
            else if (ratio >= 0.5f)
            {
                return ScoreRank.WhiteIki;
            }
            else
            {
                return ScoreRank.None;
            }
        }

        public static void CreateEnsoScoreRankIcon(ScoreRank scoreRank, int playerNo)
        {
            if (GameObject.Find("DaniDojo") == null)
            {
                Plugin.Instance.StartCoroutine(CreateEnsoScoreRankAnimation(scoreRank, playerNo));
            }
        }

        public static IEnumerator CreateEnsoScoreRankAnimation(ScoreRank scoreRank, int playerNo)
        {
            var canvasFgObject = GameObject.Find("CanvasFg");

            Vector2 MainPosition = GetScoreRankPosition(-905, 305);
            Vector2 DesiredPosition = new Vector2(-905, 305);

            // This position is changed at runtime, but the desired location is -920, 300
            // Adding 1920/2 or 1080/2 will put it at that location
            var scoreRankObject = AssetUtility.CreateImageChild(canvasFgObject, "ScoreRank", MainPosition + new Vector2(0, -50), Path.Combine(Plugin.Instance.ConfigScoreRankAssetFolderPath.Value, "Big", scoreRank.ToString() + ".png"));
            var image = scoreRankObject.GetOrAddComponent<Image>();
            var imageColor = image.color;
            imageColor.a = 0;
            image.color = imageColor;

            Plugin.Instance.StartCoroutine(AssetUtility.MoveOverSeconds(scoreRankObject, DesiredPosition, 0.25f));
            Plugin.Instance.StartCoroutine(AssetUtility.ChangeTransparencyOverSeconds(scoreRankObject, 0.25f, true));
            yield return new WaitForSeconds(0.25f);

            // Grow and shrink over 200 ms?

            yield return new WaitForSeconds(0.2f);

            // Wait 2 seconds before moving up and disappearing
            yield return new WaitForSeconds(2);

            Plugin.Instance.StartCoroutine(AssetUtility.MoveOverSeconds(scoreRankObject, DesiredPosition + new Vector2(0, 50), 0.25f));
            Plugin.Instance.StartCoroutine(AssetUtility.ChangeTransparencyOverSeconds(scoreRankObject, 0.25f, false));
            yield return new WaitForSeconds(0.5f);

            GameObject.Destroy(scoreRankObject);

        }

        private static Vector2 GetScoreRankPosition(int x, int y)
        {
            return new Vector2(x + (1920 / 2), y + (1080 / 2));
        }
        private static Vector2 GetScoreRankPosition(Vector2 input)
        {
            return new Vector2(input.x + (1920 / 2), input.y + (1080 / 2));
        }
    }
}
