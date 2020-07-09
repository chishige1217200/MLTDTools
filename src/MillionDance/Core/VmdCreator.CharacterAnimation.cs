﻿using System;
using AssetStudio.Extended.CompositeModels;
using Imas.Data.Serialized;
using JetBrains.Annotations;
using OpenMLTD.MillionDance.Entities.Pmx;
using OpenMLTD.MillionDance.Entities.Vmd;

namespace OpenMLTD.MillionDance.Core {
    partial class VmdCreator {

        [NotNull]
        public VmdMotion CreateCharacterAnimation([CanBeNull] IBodyAnimationSource bodyAnimationSource, [CanBeNull] ScenarioObject scenario, [CanBeNull] PrettyAvatar avatar, [CanBeNull] PmxModel mltdPmxModel, int formationNumber) {
            VmdBoneFrame[] frames;

            if (ProcessBoneFrames && (bodyAnimationSource != null && avatar != null && mltdPmxModel != null)) {
                frames = CreateBoneFrames(bodyAnimationSource, scenario, avatar, mltdPmxModel, formationNumber);
            } else {
                frames = Array.Empty<VmdBoneFrame>();
            }

            return new VmdMotion(ModelName, frames, null, null, null, null);
        }

    }
}
