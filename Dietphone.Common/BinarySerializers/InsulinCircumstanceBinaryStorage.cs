using System;
using System.Linq;
using Dietphone.Models;
using System.IO;

namespace Dietphone.BinarySerializers
{
    public sealed class InsulinCircumstanceBinaryStorage : BinaryStorage<InsulinCircumstance>
    {
        protected override string FileName
        {
            get
            {
                return "insulincircumstances.db";
            }
        }

        protected override byte WritingVersion
        {
            get
            {
                return 1;
            }
        }

        public override void WriteItem(BinaryWriter writer, InsulinCircumstance circumstance)
        {
            writer.Write(circumstance.Id);
            writer.Write((byte)circumstance.Kind);
            if (circumstance.Kind == InsulinCircumstanceKind.Custom)
            {
                writer.WriteString(circumstance.Name);
            }
        }

        public override void ReadItem(BinaryReader reader, InsulinCircumstance circumstance)
        {
            circumstance.Id = reader.ReadGuid();
            circumstance.Kind = (InsulinCircumstanceKind)reader.ReadByte();
            if (circumstance.Kind == InsulinCircumstanceKind.Custom)
            {
                circumstance.Name = reader.ReadString();
            }
        }
    }
}
