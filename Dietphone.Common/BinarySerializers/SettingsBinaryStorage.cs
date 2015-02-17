using Dietphone.Models;
using System.IO;

namespace Dietphone.BinarySerializers
{
    public class SettingsBinaryStorage : BinaryStorage<Settings>
    {

        protected override string FileName
        {
            get
            {
                return "settings.db";
            }
        }

        protected override byte WritingVersion
        {
            get
            {
                return 9;
            }
        }

        public override void WriteItem(BinaryWriter writer, Settings item)
        {
            writer.Write(item.ScoreEnergy);
            writer.Write(item.ScoreProtein);
            writer.Write(item.ScoreDigestibleCarbs);
            writer.Write(item.ScoreFat);
            writer.Write(item.ScoreCu);
            writer.Write(item.ScoreFpu);
            writer.WriteString(item.NextUiCulture);
            writer.WriteString(item.NextProductCulture);
            writer.Write(item.SugarsAfterInsulinHours);
            writer.Write((byte)item.SugarUnit);
            writer.Write(item.MaxBolus);
            writer.Write(item.MruProductIds);
            writer.Write(item.CloudSecret);
            writer.Write(item.CloudToken);
            writer.Write(item.CloudExportDue);
            writer.Write(item.MruProductMaxCount);
            writer.Write((byte)item.Unit);
            writer.Write(item.TrialCounter);
            writer.Write(item.ShowWelcomeScreen);
            writer.Write(item.CuSugarsHoursToExcludingPlusOneSmoothing);
            writer.Write(item.FpuSugarsHoursFromExcludingMinusOneSmoothing);
        }

        public override void ReadItem(BinaryReader reader, Settings item)
        {
            item.ScoreEnergy = reader.ReadBoolean();
            item.ScoreProtein = reader.ReadBoolean();
            item.ScoreDigestibleCarbs = reader.ReadBoolean();
            item.ScoreFat = reader.ReadBoolean();
            item.ScoreCu = reader.ReadBoolean();
            item.ScoreFpu = reader.ReadBoolean();
            if (ReadingVersion >= 3)
            {
                item.NextUiCulture = reader.ReadString();
                item.NextProductCulture = reader.ReadString();
            }
            if (ReadingVersion >= 4)
            {
                item.SugarsAfterInsulinHours = reader.ReadInt32();
                item.SugarUnit = (SugarUnit)reader.ReadByte();
                item.MaxBolus = reader.ReadSingle();
                item.MruProductIds = reader.ReadGuids();
                item.CloudSecret = reader.ReadString();
                item.CloudToken = reader.ReadString();
                item.CloudExportDue = reader.ReadDateTime();
            }
            if (ReadingVersion >= 5)
                item.MruProductMaxCount = reader.ReadByte();
            if (ReadingVersion >= 6)
                item.Unit = (Unit)reader.ReadByte();
            if (ReadingVersion >= 7)
                item.TrialCounter = reader.ReadByte();
            if (ReadingVersion >= 8)
                item.ShowWelcomeScreen = reader.ReadBoolean();
            if (ReadingVersion >= 9)
            {
                item.CuSugarsHoursToExcludingPlusOneSmoothing = reader.ReadSingle();
                item.FpuSugarsHoursFromExcludingMinusOneSmoothing = reader.ReadSingle();
            }
        }
    }
}
