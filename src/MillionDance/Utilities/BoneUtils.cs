﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using MillionDance.Core;
using MillionDance.Entities.Internal;
using MillionDance.Entities.Pmx;
using MillionDance.Extensions;
using OpenTK;
using UnityStudio.UnityEngine.Animation;

namespace MillionDance.Utilities {
    internal static class BoneUtils {

        static BoneUtils() {
            var dict = new Dictionary<string, string>();

            foreach (var kv in BonePathMap) {
                string boneName;

                if (kv.Key.Contains("BODY_SCALE/")) {
                    boneName = kv.Key.Replace("BODY_SCALE/", string.Empty);
                } else {
                    boneName = kv.Key;
                }

                dict.Add(boneName, kv.Value);
            }

            BoneNameMap = dict;
        }

        [NotNull, ItemNotNull]
        public static IReadOnlyList<BoneNode> BuildBoneHierarchy([NotNull] Avatar avatar) {
            var boneList = new List<BoneNode>();

            for (var i = 0; i < avatar.AvatarSkeleton.Nodes.Length; i++) {
                var n = avatar.AvatarSkeleton.Nodes[i];

                var parent = n.ParentIndex >= 0 ? boneList[n.ParentIndex] : null;
                var boneId = avatar.AvatarSkeleton.NodeIDs[i];
                var bonePath = avatar.BoneNamesMap[boneId];

                var boneIndex = avatar.AvatarSkeleton.NodeIDs.FindIndex(boneId);

                if (boneIndex < 0) {
                    throw new IndexOutOfRangeException();
                }

                var initialPose = avatar.AvatarSkeletonPose.Transforms[boneIndex];

                var t = initialPose.Translation.ToOpenTK() * ConversionConfig.ScaleUnityToMmd;
                var q = initialPose.Rotation.ToOpenTK();

                var bone = new BoneNode(parent, i, bonePath, t, q);

                boneList.Add(bone);
            }

#if DEBUG
            Debug.Print("Model bones:");

            for (var i = 0; i < boneList.Count; i++) {
                var bone = boneList[i];
                Debug.Print("[{0}]: {1}", i, bone.ToString());
            }
#endif

            foreach (var bone in boneList) {
                var level = 0;
                var parent = bone.Parent;

                while (parent != null) {
                    ++level;
                    parent = parent.Parent;
                }

                bone.Level = level;
            }

            foreach (var bone in boneList) {
                bone.Initialize();
            }

            return boneList;
        }

        public static IReadOnlyList<BoneNode> BuildBoneHierarchy([NotNull] PmxModel pmx) {
            var boneList = new List<BoneNode>();

            for (var i = 0; i < pmx.Bones.Count; i++) {
                var pmxBone = pmx.Bones[i];
                var parent = pmxBone.ParentIndex >= 0 ? boneList[pmxBone.ParentIndex] : null;

                var mltdBoneName = BoneNameMap.SingleOrDefault(kv => kv.Value == pmx.Name).Key;
                var path = mltdBoneName ?? pmxBone.Name;

                Vector3 t;

                if (parent != null) {
                    t = pmxBone.InitialPosition - parent.InitialPosition;
                } else {
                    t = pmxBone.InitialPosition;
                }

                var bone = new BoneNode(parent, i, path, t, Quaternion.Identity);

                boneList.Add(bone);
            }

            foreach (var bone in boneList) {
                var level = 0;
                var parent = bone.Parent;

                while (parent != null) {
                    ++level;
                    parent = parent.Parent;
                }

                bone.Level = level;
            }

            foreach (var bone in boneList) {
                bone.Initialize();
            }

            return boneList;
        }

        public static string TranslateBoneName([NotNull] string nameJp) {
            if (NameJpToEn.ContainsKey(nameJp)) {
                return NameJpToEn[nameJp];
            } else {
                return string.Empty;
            }
        }

        public static readonly IReadOnlyDictionary<string, string> BoneNameMap;

        public static readonly IReadOnlyDictionary<string, string> BonePathMap = new Dictionary<string, string> {
            ["POSITION"] = "操作中心",
            ["POSITION/SCALE_POINT"] = "全ての親",
            ["MODEL_00"] = "センター",
            ["MODEL_00/BODY_SCALE/BASE"] = "グルーブ",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI"] = "腰",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_L"] = "左足",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_L/HIZA_L"] = "左ひざ",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_L/HIZA_L/ASHI_L"] = "左足首",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_L/HIZA_L/ASHI_L/TSUMASAKI_L"] = "左つま先",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_R"] = "右足",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_R/HIZA_R"] = "右ひざ",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_R/HIZA_R/ASHI_R"] = "右足首",
            ["MODEL_00/BODY_SCALE/BASE/KOSHI/MOMO_R/HIZA_R/ASHI_R/TSUMASAKI_R"] = "右つま先",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1"] = "上半身",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2"] = "上半身2",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/KUBI"] = "首",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/KUBI/ATAMA"] = "頭",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L"] = "左肩",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L"] = "左腕",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L"] = "左ひじ",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L"] = "左手首",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/HITO3_L"] = "左人指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/HITO3_L/HITO2_L"] = "左人指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/HITO3_L/HITO2_L/HITO1_L"] = "左人指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L"] = "左ダミー",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L/KO3_L"] = "左小指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L/KO3_L/KO2_L"] = "左小指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L/KO3_L/KO2_L/KO1_L"] = "左小指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L/KUSU3_L"] = "左薬指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L/KUSU3_L/KUSU2_L"] = "左薬指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/KUKO_L/KUSU3_L/KUSU2_L/KUSU1_L"] = "左薬指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/NAKA3_L"] = "左中指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/NAKA3_L/NAKA2_L"] = "左中指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/NAKA3_L/NAKA2_L/NAKA1_L"] = "左中指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/OYA3_L"] = "左親指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/OYA3_L/OYA2_L"] = "左親指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_L/KATA_L/UDE_L/TE_L/OYA3_L/OYA2_L/OYA1_L"] = "左親指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R"] = "右肩",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R"] = "右腕",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R"] = "右ひじ",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R"] = "右手首",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/HITO3_R"] = "右人指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/HITO3_R/HITO2_R"] = "右人指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/HITO3_R/HITO2_R/HITO1_R"] = "右人指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R"] = "右ダミー",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R/KO3_R"] = "右小指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R/KO3_R/KO2_R"] = "右小指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R/KO3_R/KO2_R/KO1_R"] = "右小指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R/KUSU3_R"] = "右薬指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R/KUSU3_R/KUSU2_R"] = "右薬指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/KUKO_R/KUSU3_R/KUSU2_R/KUSU1_R"] = "右薬指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/NAKA3_R"] = "右中指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/NAKA3_R/NAKA2_R"] = "右中指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/NAKA3_R/NAKA2_R/NAKA1_R"] = "右中指３",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/OYA3_R"] = "右親指１",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/OYA3_R/OYA2_R"] = "右親指２",
            ["MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/SAKOTSU_R/KATA_R/UDE_R/TE_R/OYA3_R/OYA2_R/OYA1_R"] = "右親指３"
        };

        private static readonly IReadOnlyDictionary<string, string> NameJpToEn = new Dictionary<string, string> {
            ["操作中心"] = "view cnt",
            ["全ての親"] = "master",
            ["センター"] = "center",
            ["グルーブ"] = "groove",
            ["腰"] = "waist",
            ["左足"] = "leg_L",
            ["左ひざ"] = "knee_L",
            ["左足首"] = "angle_L",
            ["左つま先"] = "toe_L",
            ["右足"] = "leg_R",
            ["右ひざ"] = "knee_R",
            ["右足首"] = "ankle_R",
            ["右つま先"] = "toe_R",
            ["上半身"] = "upper body",
            ["上半身2"] = "upper body2",
            ["首"] = "neck",
            ["頭"] = "head",
            ["左肩"] = "shoulder_L",
            ["左腕"] = "arm_L",
            ["左ひじ"] = "elbow_L",
            ["左手首"] = "wrist_L",
            ["左人指１"] = "fore1_L",
            ["左人指２"] = "fore2_L",
            ["左人指３"] = "fore3_L",
            ["左ダミー"] = "dummy_L",
            ["左小指１"] = "little1_L",
            ["左小指２"] = "little2_L",
            ["左小指３"] = "little3_L",
            ["左薬指１"] = "third1_L",
            ["左薬指２"] = "third2_L",
            ["左薬指３"] = "third3_L",
            ["左中指１"] = "middle1_L",
            ["左中指２"] = "middle2_L",
            ["左中指３"] = "middle3_L",
            ["左親指１"] = "thumb1_L",
            ["左親指２"] = "thumb2_L",
            ["左親指３"] = "thumb3_L",
            ["右肩"] = "shoulder_R",
            ["右腕"] = "arm_R",
            ["右ひじ"] = "elbow_R",
            ["右手首"] = "wrist_R",
            ["右人指１"] = "fore1_R",
            ["右人指２"] = "fore2_R",
            ["右人指３"] = "fore3_R",
            ["右ダミー"] = "dummy_R",
            ["右小指１"] = "little1_R",
            ["右小指２"] = "little2_R",
            ["右小指３"] = "little3_R",
            ["右薬指１"] = "third1_R",
            ["右薬指２"] = "third2_R",
            ["右薬指３"] = "third3_R",
            ["右中指１"] = "middle1_R",
            ["右中指２"] = "middle2_R",
            ["右中指３"] = "middle3_R",
            ["右親指１"] = "thumb1_R",
            ["右親指２"] = "thumb2_R",
            ["右親指３"] = "thumb3_R"
        };

    }
}
