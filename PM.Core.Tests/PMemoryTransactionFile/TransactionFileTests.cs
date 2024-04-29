using FileFormatExplain;
using PM.Core.PMemory;
using PM.Core.PMemory.MemoryLayoutTransactions;
using PM.Core.PMemory.PMemoryTransactionFile;
using PM.Tests.Common;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PM.Core.Tests.PMemoryTransactionFile
{
    public class TransactionFileTests : UnitTest
    {
        private readonly ITestOutputHelper _output;

        public TransactionFileTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void OnAddNewBlockLayout_ShouldWriteOnTransactionFile()
        {
            DeleteFile(nameof(OnAddNewBlockLayout_ShouldWriteOnTransactionFile));
            var transactionFilePm = CreateTransationFile(nameof(OnAddNewBlockLayout_ShouldWriteOnTransactionFile));
            var transactionFile = new TransactionFile(transactionFilePm, new PersistentAllocatorLayout());

            transactionFile.AddNewBlockLayout(
                new AddBlockLayout(
                    startBlockOffsetField: uint.MaxValue,
                    regionsQttyField: 8,
                    regionsSizeField: 1000
                )
                { Order = new Core.PMemory.FileFields.OrderField(offset: TransactionFileOffset.AddBlockOrder, instance: 2) });
            var content = ReadFileContent(transactionFilePm.FilePath);
            // 010100010100ffffffff08e803000000
            // CommitByte=01
            // Version=0001
            // =================AddBlockLayout=================
            // CommitByte=01
            // Order=0001
            // StartBlockOffset=FFFFFFFF
            // RegionsQtty=08
            // RegionSize=000003E8
            // =================RemoveBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // =================UpdateContentBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // ContentSize=00000000
            // Content=
            var result = TransactionFileDecoder.Decode_HexResponse(content);
            _output.WriteLine(result);
            Assert.StartsWith("010100010100ffffffff08e803000000", ByteArrayToHexStringConverter.ByteArrayToString(content));


            transactionFile.AddNewBlockLayout(
                new AddBlockLayout(
                    startBlockOffsetField: uint.MinValue,
                    regionsQttyField: 16,
                    regionsSizeField: 1001
                )
                { Order = new Core.PMemory.FileFields.OrderField(offset: TransactionFileOffset.AddBlockOrder, instance: 2) });

            content = ReadFileContent(transactionFilePm.FilePath);
            // 0101000102000000000010e90300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
            // CommitByte=01
            // Version=0001
            // =================AddBlockLayout=================
            // CommitByte=01
            // Order=0002
            // StartBlockOffset=00000000
            // RegionsQtty=10
            // RegionSize=000003E9
            // =================RemoveBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // =================UpdateContentBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // ContentSize=00000000
            // Content=
            result = TransactionFileDecoder.Decode_HexResponse(content);
            _output.WriteLine(result);
            Assert.StartsWith("0101000102000000000010e90300000", ByteArrayToHexStringConverter.ByteArrayToString(content));
        }
        
        [Fact]
        public void OnRemoveBlockLayout_ShouldWriteOnTransactionFile()
        {
            DeleteFile(nameof(OnRemoveBlockLayout_ShouldWriteOnTransactionFile));
            var transactionFilePm = CreateTransationFile(nameof(OnRemoveBlockLayout_ShouldWriteOnTransactionFile));
            var transactionFile = new TransactionFile(transactionFilePm, new PersistentAllocatorLayout());

            transactionFile.AddRemoveBlockLayout(
                new RemoveBlockLayout(
                    startBlockOffsetField: uint.MaxValue
                ));
            var content = ReadFileContent(transactionFilePm.FilePath);
            // 010100000000000000000000000000010100ffffffff00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
            // CommitByte=01
            // Version=0001
            // =================AddBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // RegionsQtty=00
            // RegionSize=00000000
            // =================RemoveBlockLayout=================
            // CommitByte=01
            // Order=0001
            // StartBlockOffset=FFFFFFFF
            // =================UpdateContentBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // ContentSize=00000000
            // Content=
            var result = TransactionFileDecoder.Decode_HexResponse(content);
            _output.WriteLine(result);
            Assert.StartsWith("010100000000000000000000000000010100ffffffff0000", ByteArrayToHexStringConverter.ByteArrayToString(content));


            transactionFile.AddRemoveBlockLayout(
                new RemoveBlockLayout(
                    startBlockOffsetField: uint.MinValue
                ));

            content = ReadFileContent(transactionFilePm.FilePath);
            // 0101000000000000000000000000000102000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
            // CommitByte=01
            // Version=0001
            // =================AddBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // RegionsQtty=00
            // RegionSize=00000000
            // =================RemoveBlockLayout=================
            // CommitByte=01
            // Order=0002
            // StartBlockOffset=00000000
            // =================UpdateContentBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // ContentSize=00000000
            // Content=
            result = TransactionFileDecoder.Decode_HexResponse(content);
            _output.WriteLine(result);
            Assert.StartsWith("01010000000000000000000000000001020", ByteArrayToHexStringConverter.ByteArrayToString(content));
        }
        
        [Fact]
        public void UpdateContentBlockLayout_ShouldWriteOnTransactionFile()
        {
            DeleteFile(nameof(UpdateContentBlockLayout_ShouldWriteOnTransactionFile));
            var transactionFilePm = CreateTransationFile(nameof(UpdateContentBlockLayout_ShouldWriteOnTransactionFile));
            var transactionFile = new TransactionFile(transactionFilePm, new PersistentAllocatorLayout());

            var content = new byte[] { 1, 2, 3, 4, 5, 6 };
            transactionFile.AddUpdateContentBlockLayout(
                new UpdateContentBlockLayout(
                    startBlockOffset: uint.MaxValue,
                    contentSize: (uint)content.Length,
                    content: content
                )
                { Order = new Core.PMemory.FileFields.OrderField(offset: TransactionFileOffset.AddBlockOrder, instance: 3) });
            var contentFromFile = ReadFileContent(transactionFilePm.FilePath);
            // 01010000000000000000000000000000000000000000010100ffffffff060000000102030405060000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
            // CommitByte=01
            // Version=0001
            // =================AddBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // RegionsQtty=00
            // RegionSize=00000000
            // =================RemoveBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // =================UpdateContentBlockLayout=================
            // CommitByte=01
            // Order=0001
            // StartBlockOffset=FFFFFFFF
            // ContentSize=00000006
            // Content=010203040506
            var result = TransactionFileDecoder.Decode_HexResponse(contentFromFile);
            _output.WriteLine(result);
            Assert.StartsWith("01010000000000000000000000000000000000000000010100ffffffff06000000010203040506000", ByteArrayToHexStringConverter.ByteArrayToString(contentFromFile));


            content = Encoding.ASCII.GetBytes(@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse accumsan feugiat euismod. In iaculis nisi vitae condimentum luctus. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Vestibulum tincidunt, lectus vel sollicitudin semper, mi lacus tempor lacus, a volutpat odio mi vitae dui. Aenean vitae tellus elit. Fusce sit amet cursus mauris. Ut convallis libero orci, quis mattis risus sodales maximus. Nullam neque lorem, consectetur in nisi eu, tincidunt pellentesque leo. Praesent ac ipsum in ante sodales finibus nec vel elit. Etiam fringilla enim pellentesque tempus lobortis. Pellentesque quis mattis tortor, sed fermentum nulla. Phasellus at euismod.");
            transactionFile.AddUpdateContentBlockLayout(
                new UpdateContentBlockLayout(
                    startBlockOffset: uint.MaxValue,
                    contentSize: (uint)content.Length,
                    content: content
                )
                { Order = new Core.PMemory.FileFields.OrderField(offset: TransactionFileOffset.AddBlockOrder, instance: 3) });

            contentFromFile = ReadFileContent(transactionFilePm.FilePath);
            // 01010000000000000000000000000000000000000000010200ffffffffbe0200004c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742e2053757370656e646973736520616363756d73616e206665756769617420657569736d6f642e20496e20696163756c6973206e69736920766974616520636f6e64696d656e74756d206c75637475732e204f72636920766172697573206e61746f7175652070656e617469627573206574206d61676e6973206469732070617274757269656e74206d6f6e7465732c206e61736365747572207269646963756c7573206d75732e20566573746962756c756d2074696e636964756e742c206c65637475732076656c20736f6c6c696369747564696e2073656d7065722c206d69206c616375732074656d706f72206c616375732c206120766f6c7574706174206f64696f206d69207669746165206475692e2041656e65616e2076697461652074656c6c757320656c69742e2046757363652073697420616d657420637572737573206d61757269732e20557420636f6e76616c6c6973206c696265726f206f7263692c2071756973206d617474697320726973757320736f64616c6573206d6178696d75732e204e756c6c616d206e65717565206c6f72656d2c20636f6e736563746574757220696e206e6973692065752c2074696e636964756e742070656c6c656e746573717565206c656f2e205072616573656e7420616320697073756d20696e20616e746520736f64616c65732066696e69627573206e65632076656c20656c69742e20457469616d206672696e67696c6c6120656e696d2070656c6c656e7465737175652074656d707573206c6f626f727469732e2050656c6c656e7465737175652071756973206d617474697320746f72746f722c20736564206665726d656e74756d206e756c6c612e2050686173656c6c757320617420657569736d6f642e00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
            // CommitByte=01
            // Version=0001
            // =================AddBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // RegionsQtty=00
            // RegionSize=00000000
            // =================RemoveBlockLayout=================
            // CommitByte=00
            // Order=0000
            // StartBlockOffset=00000000
            // =================UpdateContentBlockLayout=================
            // CommitByte=01
            // Order=0002
            // StartBlockOffset=FFFFFFFF
            // ContentSize=000002BE
            // Content=4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742e2053757370656e646973736520616363756d73616e206665756769617420657569736d6f642e20496e20696163756c6973206e69736920766974616520636f6e64696d656e74756d206c75637475732e204f72636920766172697573206e61746f7175652070656e617469627573206574206d61676e6973206469732070617274757269656e74206d6f6e7465732c206e61736365747572207269646963756c7573206d75732e20566573746962756c756d2074696e636964756e742c206c65637475732076656c20736f6c6c696369747564696e2073656d7065722c206d69206c616375732074656d706f72206c616375732c206120766f6c7574706174206f64696f206d69207669746165206475692e2041656e65616e2076697461652074656c6c757320656c69742e2046757363652073697420616d657420637572737573206d61757269732e20557420636f6e76616c6c6973206c696265726f206f7263692c2071756973206d617474697320726973757320736f64616c6573206d6178696d75732e204e756c6c616d206e65717565206c6f72656d2c20636f6e736563746574757220696e206e6973692065752c2074696e636964756e742070656c6c656e746573717565206c656f2e205072616573656e7420616320697073756d20696e20616e746520736f64616c65732066696e69627573206e65632076656c20656c69742e20457469616d206672696e67696c6c6120656e696d2070656c6c656e7465737175652074656d707573206c6f626f727469732e2050656c6c656e7465737175652071756973206d617474697320746f72746f722c20736564206665726d656e74756d206e756c6c612e2050686173656c6c757320617420657569736d6f642e
            result = TransactionFileDecoder.Decode_HexResponse(contentFromFile);
            _output.WriteLine(result);
            Assert.StartsWith("01010000000000000000000000000000000000000000010200ffffffffbe0200004c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742e2053757370656e646973736520616363756d73616e206665756769617420657569736d6f642e20496e20696163756c6973206e69736920766974616520636f6e64696d656e74756d206c75637475732e204f72636920766172697573206e61746f7175652070656e617469627573206574206d61676e6973206469732070617274757269656e74206d6f6e7465732c206e61736365747572207269646963756c7573206d75732e20566573746962756c756d2074696e636964756e742c206c65637475732076656c20736f6c6c696369747564696e2073656d7065722c206d69206c616375732074656d706f72206c616375732c206120766f6c7574706174206f64696f206d69207669746165206475692e2041656e65616e2076697461652074656c6c757320656c69742e2046757363652073697420616d657420637572737573206d61757269732e20557420636f6e76616c6c6973206c696265726f206f7263692c2071756973206d617474697320726973757320736f64616c6573206d6178696d75732e204e756c6c616d206e65717565206c6f72656d2c20636f6e736563746574757220696e206e6973692065752c2074696e636964756e742070656c6c656e746573717565206c656f2e205072616573656e7420616320697073756d20696e20616e746520736f64616c65732066696e69627573206e65632076656c20656c69742e20457469616d206672696e67696c6c6120656e696d2070656c6c656e7465737175652074656d707573206c6f626f727469732e2050656c6c656e7465737175652071756973206d617474697320746f72746f722c20736564206665726d656e74756d206e756c6c612e2050686173656c6c757320617420657569736d6f642e", ByteArrayToHexStringConverter.ByteArrayToString(contentFromFile));
        }

        private static byte[] ReadFileContent(string filePath)
        {
            using var stream = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            using var memstream = new MemoryStream();
            stream.BaseStream.CopyTo(memstream);
            return memstream.ToArray();
        }

        private static PmCSharpDefinedTypes CreateTransationFile(string methodName)
        {
            return new PmCSharpDefinedTypes(CreatePmStream(methodName, 200));
        }
    }
}
