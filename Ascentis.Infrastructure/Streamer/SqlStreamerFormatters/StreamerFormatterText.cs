﻿using System.Globalization;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class StreamerFormatterText : StreamerFormatter
    {
        protected string FormatString { get; set; }
        protected byte[] WriteBuffer { get; set; }

        public string[] ColumnFormatStrings { get; set; }
        public Encoding OutputEncoding { get; set; } = Encoding.UTF8;
        public CultureInfo FormatCultureInfo { get; set; } = CultureInfo.InvariantCulture;

        protected virtual byte[] RowToBytes(object[] row, out int bytesWritten)
        {
            var buf = WriteBuffer;
            var s = string.Format(FormatCultureInfo, FormatString, row);
            bytesWritten = s.Length;
            if (buf != null)
            {
                if (bytesWritten > buf.Length)
                    buf = new byte[(int)(bytesWritten * 1.25)];
                OutputEncoding.GetBytes(s, 0, s.Length, buf, 0);
            }
            else
                buf = OutputEncoding.GetBytes(s);
            return buf;
        }

        protected string ColumnFormatString(int index)
        {
            return ColumnFormatStrings != null && ColumnFormatStrings[index] != "" ? ":" + ColumnFormatStrings[index] : "";
        }

        public override void Prepare(IStreamerAdapter source, object target)
        {
            base.Prepare(source, target);

            if (ColumnFormatStrings != null && ColumnFormatStrings.Length != Source.FieldCount)
                throw new StreamerFormatterException("When ColumnFormatStrings is provided its length must match result set field count");
        }

        public override void Process(object[] row, object target)
        {
            var bytes = RowToBytes(row, out var bytesWritten);
            ((Stream)target).Write(bytes, 0, bytesWritten);
        }
    }
}
