﻿using System.IO;
using System.Collections.Generic;

namespace Librarian
{
    class ChapterList
    {
        List<Chapter> m_chapters;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public ChapterList (Book book)
        {
            using (var fileStream = new FileStream (book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek (Book.HEADER_LENGTH, SeekOrigin.Begin);
                var reader = new BinaryReader (fileStream);

                uint firstChapterPosition   = reader.ReadUInt32 ();
                int chapterCount            = (int)(firstChapterPosition - Book.HEADER_LENGTH) / 4 - 1;

                m_chapters = new List<Chapter> (chapterCount);

                uint previousChapterEndPosition = firstChapterPosition;

                for (int i = 0; i < chapterCount; i++)
                {
                    uint chapterEndPosition = reader.ReadUInt32 ();
                    m_chapters.Add (new Chapter (previousChapterEndPosition, chapterEndPosition));
                    previousChapterEndPosition = chapterEndPosition;
                }
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public Chapter this[int index]
        {
            get { return m_chapters[index]; }
        }
    }
}
