using System.IO;
using System.Collections.Generic;

namespace Librarian.Wdt
{
    public class ChapterList
    {
        public int Count { get { return m_chapters.Count; } }

        List<Chapter> m_chapters;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public ChapterList (WdtFile wdtFile)
        {
            using (var fileStream = new FileStream (wdtFile.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek (WdtFile.HEADER_LENGTH, SeekOrigin.Begin);
                var reader = new BinaryReader (fileStream);

                int firstChapterPosition   = reader.ReadInt32 ();
                int chapterCount           = (firstChapterPosition - WdtFile.HEADER_LENGTH) / 4 - 1;

                m_chapters = new List<Chapter> (chapterCount);

                int previousChapterEndPosition = firstChapterPosition;

                for (int i = 0; i < chapterCount; i++)
                {
                    int chapterEndPosition = reader.ReadInt32 ();
                    m_chapters.Add (new Chapter (previousChapterEndPosition, chapterEndPosition));
                    previousChapterEndPosition = chapterEndPosition;
                }

                m_chapters.Add (new Chapter (previousChapterEndPosition, (int)fileStream.Length));     // Last chapter end position = wdtFile end
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public Chapter this[int index]
        {
            get { return m_chapters[index]; }
        }
    }
}
