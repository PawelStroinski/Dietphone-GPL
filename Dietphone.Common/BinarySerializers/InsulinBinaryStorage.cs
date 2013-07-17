using System;
using System.Linq;
using Dietphone.Models;
using System.IO;

namespace Dietphone.BinarySerializers
{
    public sealed class InsulinBinaryStorage : BinaryStorage<Insulin>
    {
        protected override string FileName
        {
            get
            {
                return "insulins.db";
            }
        }

        protected override byte WritingVersion
        {
            get
            {
                return 1;
            }
        }

        public override void WriteItem(BinaryWriter writer, Insulin insulin)
        {
            writer.Write(insulin.Id);
            writer.Write(insulin.DateTime);
            writer.WriteString(insulin.Note);
            writer.Write(insulin.NormalBolus);
            writer.Write(insulin.SquareWaveBolus);
            writer.Write(insulin.SquareWaveBolusHours);
            writer.Write(insulin.ReadCircumstances());
        }

        public override void ReadItem(BinaryReader reader, Insulin insulin)
        {
            insulin.Id = reader.ReadGuid();
            insulin.DateTime = reader.ReadDateTime();
            insulin.Note = reader.ReadString();
            insulin.NormalBolus = reader.ReadSingle();
            insulin.SquareWaveBolus = reader.ReadSingle();
            insulin.SquareWaveBolusHours = reader.ReadSingle();
            insulin.InitializeCircumstances(reader.ReadGuids());
        }
    }
}
