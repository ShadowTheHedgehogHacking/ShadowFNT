using ShadowFNT.Structures;
using Xunit;

namespace ShadowFNTTest
{
    public class Parsing
    {

        [Fact]
        public void ReadCleanFNT()
        {
            var fileName = "Assets/stg0100_EN.fnt";
            var filterString = "Assets/";
            var file = Assets.Assets.CleanSTG0100EN();
            Assert.NotNull(file);
            var fnt = FNT.ParseFNTFile(fileName, ref file, filterString);
            Assert.Equal(fileName, fnt.fileName);
            Assert.Equal(filterString, fnt.filterString);
            Assert.Equal(471, fnt.entryTable.Count);
        }
    }
}
