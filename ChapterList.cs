using System.IO;
using System.Collections.Generic;

namespace Librarian
{
    class ChapterList
    {
        public int Count { get { return m_chapters.Count; } }

        List<Chapter> m_chapters;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public ChapterList (Book book)
        {
            using (var fileStream = new FileStream (book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek (Book.HEADER_LENGTH, SeekOrigin.Begin);
                var reader = new BinaryReader (fileStream);

                int firstChapterPosition   = reader.ReadInt32 ();
                int chapterCount           = (firstChapterPosition - Book.HEADER_LENGTH) / 4 - 1;

                m_chapters = new List<Chapter> (chapterCount);

                int previousChapterEndPosition = firstChapterPosition;

                for (int i = 0; i < chapterCount; i++)
                {
                    int chapterEndPosition = reader.ReadInt32 ();
                    m_chapters.Add (new Chapter (previousChapterEndPosition, chapterEndPosition));
                    previousChapterEndPosition = chapterEndPosition;
                }

                m_chapters.Add (new Chapter (previousChapterEndPosition, (int)fileStream.Length));     // Last chapter end position = book end
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public Chapter this[int index]
        {
            get { return m_chapters[index]; }
        }
    }
}
