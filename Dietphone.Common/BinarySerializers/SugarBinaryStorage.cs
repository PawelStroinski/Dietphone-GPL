using Dietphone.Models;
using System.IO;

namespace Dietphone.BinarySerializers
{
    public sealed class SugarBinaryStorage : BinaryStorage<Sugar>
    {
        protected override string FileName
        {
            get
            {
                return "sugars.db";
            }
        }

        protected override byte WritingVersion
        {
            get
            {
                return 1;
            }
        }

        public override void WriteItem(BinaryWriter writer, Sugar sugar)
        {
            writer.Write(sugar.DateTime);
            writer.Write(sugar.BloodSugar);
        }

        public override void ReadItem(BinaryReader reader, Sugar sugar)
        {
            sugar.DateTime = reader.ReadDateTime();
            sugar.BloodSugar = reader.ReadSingle();
        }
    }
}
