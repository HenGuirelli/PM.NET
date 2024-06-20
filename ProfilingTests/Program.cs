
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;
using ProfilingTests;

const int IntValueToWrite = int.MaxValue;
const long LongValueToWrite = long.MaxValue;
const short ShortValueToWrite = short.MaxValue;
const byte ByteValueToWrite = byte.MaxValue;
const double DoubleValueToWrite = double.MaxValue;
const float FloatValueToWrite = float.MinValue;
const decimal DecimalValueToWrite = decimal.MaxValue;
const char CharValueToWrite = char.MaxValue;
const bool BoolValueToWrite = true;

PmGlobalConfiguration.PmTarget = PM.Core.PmTargets.TraditionalMemoryMappedFile;
PmGlobalConfiguration.PmInternalsFolder = "./ProflingTests";

var _persistentFactorySSD = new PersistentFactory();
var _proxy = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");


while (true)
{
    _proxy.IntVal = IntValueToWrite;
    _proxy.LongVal = LongValueToWrite;
    _proxy.ShortVal = ShortValueToWrite;
    _proxy.ByteVal = ByteValueToWrite;
    _proxy.DoubleVal = DoubleValueToWrite;
    _proxy.FloatVal = FloatValueToWrite;
    _proxy.DecimalVal = DecimalValueToWrite;
    _proxy.CharVal = CharValueToWrite;
    _proxy.BoolVal = BoolValueToWrite;
}
