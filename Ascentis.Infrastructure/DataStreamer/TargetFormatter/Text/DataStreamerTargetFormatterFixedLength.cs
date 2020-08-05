﻿using System;
using System.IO;
using Ascentis.Infrastructure.DataStreamer.Exceptions;

namespace Ascentis.Infrastructure.DataStreamer.TargetFormatter.Text
{
    public class DataStreamerTargetFormatterFixedLength : DataStreamerTargetFormatterText
    {
        public enum OverflowStringFieldWidthBehavior { Error, Truncate }
        public int[] FieldSizes { get; set; }
        public OverflowStringFieldWidthBehavior[] OverflowStringFieldWidthBehaviors { get; set; }

        private int _rowSize;

        public override void Prepare(IDataStreamerSourceAdapter<object[]> source, Stream target)
        {
            const string crLf = "\r\n";
            base.Prepare(source, target);

            ArgsChecker.CheckForNull<NullReferenceException>(FieldSizes, nameof(FieldSizes));
            if (FieldSizes.Length != Source.FieldCount)
                throw new DataStreamerException("Provided FieldSizes array has a different length than result set column count");
            if (OverflowStringFieldWidthBehaviors != null && OverflowStringFieldWidthBehaviors.Length != Source.FieldCount)
                throw new DataStreamerException("When OverflowStringFieldWidthBehaviors is provided its length must match result set field count");

            _rowSize = 0;
            FormatString = "";
            for (var i = 0; i < Source.FieldCount; i++)
            {
                _rowSize += Math.Abs(FieldSizes[i]);
                FormatString += $"{{{i},{FieldSizes[i]}{ColumnFormatString(i)}}}";
            }

            _rowSize += crLf.Length;
            WriteBuffer = new byte[_rowSize];
            FormatString += crLf;
        }

        protected override byte[] RowToBytes(object[] row, out int bytesWritten)
        {
            var buf = base.RowToBytes(row, out bytesWritten);
            if (bytesWritten > _rowSize)
                throw new DataStreamerException("Total row size exceeds specified row size based on fixed column widths");
            return buf;
        }

        public override void Process(object[] row)
        {
            if (OverflowStringFieldWidthBehaviors != null)
                for (var i = 0; i < row.Length; i++)
                {
                    if (!(row[i] is string) || ((string) row[i]).Length <= Math.Abs(FieldSizes[i]))
                        continue;
                    var strValue = (string) row[i];
                    if (OverflowStringFieldWidthBehaviors[i] == OverflowStringFieldWidthBehavior.Error)
                        throw new DataStreamerException(
                            $"Field {Source.ColumnMetadatas[i].ColumnName} size overflow streaming using fixed length streamer");
                    row[i] = strValue.Remove(FieldSizes[i], strValue.Length - FieldSizes[i]);
                }

            base.Process(row);
        }
    }
}
