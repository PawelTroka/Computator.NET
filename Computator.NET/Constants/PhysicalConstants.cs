// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
namespace Computator.NET.Constants
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    internal static class PhysicalConstants
    {
        #region Universal

        public static readonly double c = 299792458;

        public static readonly double g = 9.80625;

        public static readonly double h = 6.6260695729*1e-34;

        public static readonly double G = 6.6738480*1e-11;

        public static readonly double MagneticConstant = 4*System.Math.PI*1e-7;

        public static readonly double ElectricConstant = 8.854187817*1e-12;

        public static readonly double ImpedanceOfVacuum = 376.730313461;
        public static readonly double PlanckLength = 1.61619997*1e-35;
        public static readonly double PlanckMass = 2.1765113*1e-8;
        public static readonly double PlanckTemperature = 1.416833e32;
        public static readonly double PlanckTime = 5.3910632*1e-44;

        public static readonly double CharacteristicImpedanceOfVacuum = 376.730313461;

        public static readonly double NewtonianConstantOfGravitation = 6.67384e-11;
        public static readonly double NewtonianConstantOfGravitationOverHBarC = 6.70837e-39;

        public static readonly double PlanckConstant = 6.62606957e-34;
        public static readonly double PlanckConstantInEvS = 4.135667516e-15;
        public static readonly double PlanckConstantOver2Pi = 1.054571726e-34;
        public static readonly double PlanckConstantOver2PiInEvS = 6.58211928e-16;

        public static readonly double PlanckMassEnergyEquivalentInGev = 1.220932e19;

        #endregion

        #region Electromagnetic constants

        public static readonly double BohrMagneton = 927.400968e-26;
        public static readonly double BohrMagnetonInEvperT = 5.7883818066e-5;
        public static readonly double BohrMagnetonInHzperT = 13.99624555e9;
        public static readonly double BohrMagnetonInInverseMetersPerTesla = 46.6864498;
        public static readonly double BohrMagnetonInKperT = 0.67171388;
        public static readonly double ConductanceQuantum = 7.7480917346e-5;
        public static readonly double ElementaryCharge = 1.602176565e-19;
        public static readonly double ElementaryChargeOverH = 2.417989348e14;
        public static readonly double InverseOfConductanceQuantum = 12906.4037217;
        public static readonly double JosephsonConstant = 483597.870e9;
        public static readonly double MagneticFluxQuantum = 2.067833758e-15;
        public static readonly double NuclearMagneton = 5.05078353e-27;
        public static readonly double NuclearMagnetonInEvperT = 3.1524512605e-8;
        public static readonly double NuclearMagnetonInInverseMetersPerTesla = 2.542623527e-2;
        public static readonly double NuclearMagnetonInKperT = 3.6582682e-4;
        public static readonly double NuclearMagnetonInMhzperT = 7.62259357;
        public static readonly double VonKlitzingConstant = 25812.8074434;

        #endregion

        #region Atomic and nuclear constants

        public static readonly double AlphaParticleMass = 6.64465675e-27;
        public static readonly double AlphaParticleMassEnergyEquivalent = 5.97191967e-10;
        public static readonly double AlphaParticleMassEnergyEquivalentInMeV = 3727.379240;
        public static readonly double AlphaParticleMassInU = 4.001506179125;
        public static readonly double AlphaParticleMolarMass = 4.001506179125e-3;
        public static readonly double AlphaParticleElectronMassRatio = 7294.2995361;
        public static readonly double AlphaParticleProtonMassRatio = 3.97259968933;

        public static readonly double BohrRadius = 0.52917721092e-10;

        public static readonly double ClassicalElectronRadius = 2.8179403267e-15;

        public static readonly double ComptonWavelength = 2.4263102389e-12;
        public static readonly double ComptonWavelengthOver2Pi = 386.15926800e-15;

        public static readonly double DeuteronElectronMagneticMomentRatio = -4.664345537e-4;
        public static readonly double DeuteronElectronMassRatio = 3670.4829652;
        public static readonly double DeuteronGFactor = 0.8574382308;
        public static readonly double DeuteronMagneticMoment = 0.433073489e-26;
        public static readonly double DeuteronMagneticMomentToBohrMagnetonRatio = 0.4669754556e-3;
        public static readonly double DeuteronMagneticMomentToNuclearMagnetonRatio = 0.8574382308;
        public static readonly double DeuteronMass = 3.34358348e-27;
        public static readonly double DeuteronMassEnergyEquivalent = 3.00506297e-10;
        public static readonly double DeuteronMassEnergyEquivalentInMeV = 1875.612859;
        public static readonly double DeuteronMassInU = 2.013553212712;
        public static readonly double DeuteronMolarMass = 2.013553212712e-3;
        public static readonly double DeuteronNeutronMagneticMomentRatio = -0.44820652;
        public static readonly double DeuteronProtonMagneticMomentRatio = 0.3070122070;
        public static readonly double DeuteronProtonMassRatio = 1.99900750097;
        public static readonly double DeuteronRmsChargeRadius = 2.1424e-15;

        public static readonly double ElectronChargeToMassQuotient = -1.758820088e11;
        public static readonly double ElectronDeuteronMagneticMomentRatio = -2143.923498;
        public static readonly double ElectronDeuteronMassRatio = 2.7244371095e-4;
        public static readonly double ElectronGFactor = -2.00231930436153;
        public static readonly double ElectronGyromagneticRatio = 1.760859708e11;
        public static readonly double ElectronGyromagneticRatioOver2Pi = 28024.95266;
        public static readonly double ElectronHelionMassRatio = 1.8195430761e-4;
        public static readonly double ElectronMagneticMoment = -928.476430e-26;
        public static readonly double ElectronMagneticMomentAnomaly = 1.15965218076e-3;
        public static readonly double ElectronMagneticMomentToBohrMagnetonRatio = -1.00115965218076;
        public static readonly double ElectronMagneticMomentToNuclearMagnetonRatio = -1838.28197090;
        public static readonly double ElectronMass = 9.10938291e-31;
        public static readonly double ElectronMassEnergyEquivalent = 8.18710506e-14;
        public static readonly double ElectronMassEnergyEquivalentInMeV = 0.510998928;
        public static readonly double ElectronMassInU = 5.4857990946e-4;
        public static readonly double ElectronMolarMass = 5.4857990946e-7;
        public static readonly double ElectronMuonMagneticMomentRatio = 206.7669896;
        public static readonly double ElectronMuonMassRatio = 4.83633166e-3;
        public static readonly double ElectronNeutronMagneticMomentRatio = 960.92050;
        public static readonly double ElectronNeutronMassRatio = 5.4386734461e-4;
        public static readonly double ElectronProtonMagneticMomentRatio = -658.2106848;
        public static readonly double ElectronProtonMassRatio = 5.4461702178e-4;
        public static readonly double ElectronTauMassRatio = 2.87592e-4;
        public static readonly double ElectronToAlphaParticleMassRatio = 1.37093355578e-4;
        public static readonly double ElectronToShieldedHelionMagneticMomentRatio = 864.058257;
        public static readonly double ElectronToShieldedProtonMagneticMomentRatio = -658.2275971;
        public static readonly double ElectronTritonMassRatio = 1.8192000653e-4;
        public static readonly double ElectronVoltAtomicMassUnitRelationship = 1.073544150e-9;
        public static readonly double ElectronVoltHartreeRelationship = 3.674932379e-2;
        public static readonly double ElectronVoltHertzRelationship = 2.417989348e14;
        public static readonly double ElectronVoltInverseMeterRelationship = 8.06554429e5;
        public static readonly double ElectronVoltJouleRelationship = 1.602176565e-19;
        public static readonly double ElectronVoltKelvinRelationship = 1.1604519e4;
        public static readonly double ElectronVoltKilogramRelationship = 1.782661845e-36;

        public static readonly double FermiCouplingConstant = 1.166364e-5;
        public static readonly double FineStructureConstant = 7.2973525698e-3;

        public static readonly double HartreeEnergy = 4.35974434e-18;
        public static readonly double HartreeEnergyInEv = 27.21138505;

        public static readonly double HelionElectronMassRatio = 5495.8852754;
        public static readonly double HelionGFactor = -4.255250613;
        public static readonly double HelionMagneticMoment = -1.074617486e-26;
        public static readonly double HelionMagneticMomentToBohrMagnetonRatio = -1.158740958e-3;
        public static readonly double HelionMagneticMomentToNuclearMagnetonRatio = -2.127625306;
        public static readonly double HelionMass = 5.00641234e-27;
        public static readonly double HelionMassEnergyEquivalent = 4.49953902e-10;
        public static readonly double HelionMassEnergyEquivalentInMeV = 2808.391482;
        public static readonly double HelionMassInU = 3.0149322468;
        public static readonly double HelionMolarMass = 3.0149322468e-3;
        public static readonly double HelionProtonMassRatio = 2.9931526707;

        public static readonly double InverseFineStructureConstant = 137.035999074;

        public static readonly double MuonComptonWavelength = 11.73444103e-15;
        public static readonly double MuonComptonWavelengthOver2Pi = 1.867594294e-15;
        public static readonly double MuonElectronMassRatio = 206.7682843;
        public static readonly double MuonGFactor = -2.0023318418;
        public static readonly double MuonMagneticMoment = -4.49044807e-26;
        public static readonly double MuonMagneticMomentAnomaly = 1.16592091e-3;
        public static readonly double MuonMagneticMomentToBohrMagnetonRatio = -4.84197044e-3;
        public static readonly double MuonMagneticMomentToNuclearMagnetonRatio = -8.89059697;
        public static readonly double MuonMass = 1.883531475e-28;
        public static readonly double MuonMassEnergyEquivalent = 1.692833667e-11;
        public static readonly double MuonMassEnergyEquivalentInMeV = 105.6583715;
        public static readonly double MuonMassInU = 0.1134289267;
        public static readonly double MuonMolarMass = 0.1134289267e-3;
        public static readonly double MuonNeutronMassRatio = 0.1124545177;
        public static readonly double MuonProtonMagneticMomentRatio = -3.183345107;
        public static readonly double MuonProtonMassRatio = 0.1126095272;
        public static readonly double MuonTauMassRatio = 5.94649e-2;

        public static readonly double NeutronComptonWavelength = 1.3195909068e-15;
        public static readonly double NeutronComptonWavelengthOver2Pi = 0.21001941568e-15;
        public static readonly double NeutronElectronMagneticMomentRatio = 1.04066882e-3;
        public static readonly double NeutronElectronMassRatio = 1838.6836605;
        public static readonly double NeutronGFactor = -3.82608545;
        public static readonly double NeutronGyromagneticRatio = 1.83247179e8;
        public static readonly double NeutronGyromagneticRatioOver2Pi = 29.1646943;
        public static readonly double NeutronMagneticMoment = -0.96623647e-26;
        public static readonly double NeutronMagneticMomentToBohrMagnetonRatio = -1.04187563e-3;
        public static readonly double NeutronMagneticMomentToNuclearMagnetonRatio = -1.91304272;
        public static readonly double NeutronMass = 1.674927351e-27;
        public static readonly double NeutronMassEnergyEquivalent = 1.505349631e-10;
        public static readonly double NeutronMassEnergyEquivalentInMeV = 939.565379;
        public static readonly double NeutronMassInU = 1.00866491600;
        public static readonly double NeutronMolarMass = 1.00866491600e-3;
        public static readonly double NeutronMuonMassRatio = 8.89248400;
        public static readonly double NeutronProtonMagneticMomentRatio = -0.68497934;
        public static readonly double NeutronProtonMassDifference = 2.30557392e-30;
        public static readonly double NeutronProtonMassDifferenceEnergyEquivalent = 2.07214650e-13;
        public static readonly double NeutronProtonMassDifferenceEnergyEquivalentInMeV = 1.29333217;
        public static readonly double NeutronProtonMassDifferenceInU = 0.00138844919;
        public static readonly double NeutronProtonMassRatio = 1.00137841917;
        public static readonly double NeutronTauMassRatio = 0.528790;
        public static readonly double NeutronToShieldedProtonMagneticMomentRatio = -0.68499694;

        public static readonly double ProtonChargeToMassQuotient = 9.57883358e7;
        public static readonly double ProtonComptonWavelength = 1.32140985623e-15;
        public static readonly double ProtonComptonWavelengthOver2Pi = 0.21030891047e-15;
        public static readonly double ProtonElectronMassRatio = 1836.15267245;
        public static readonly double ProtonGFactor = 5.585694713;
        public static readonly double ProtonGyromagneticRatio = 2.675222005e8;
        public static readonly double ProtonGyromagneticRatioOver2Pi = 42.5774806;
        public static readonly double ProtonMagneticMoment = 1.410606743e-26;
        public static readonly double ProtonMagneticMomentToBohrMagnetonRatio = 1.521032210e-3;
        public static readonly double ProtonMagneticMomentToNuclearMagnetonRatio = 2.792847356;
        public static readonly double ProtonMagneticShieldingCorrection = 25.694e-6;
        public static readonly double ProtonMass = 1.672621777e-27;
        public static readonly double ProtonMassEnergyEquivalent = 1.503277484e-10;
        public static readonly double ProtonMassEnergyEquivalentInMeV = 938.272046;
        public static readonly double ProtonMassInU = 1.007276466812;
        public static readonly double ProtonMolarMass = 1.007276466812e-3;
        public static readonly double ProtonMuonMassRatio = 8.88024331;
        public static readonly double ProtonNeutronMagneticMomentRatio = -1.45989806;
        public static readonly double ProtonNeutronMassRatio = 0.99862347826;
        public static readonly double ProtonRmsChargeRadius = 0.8775e-15;
        public static readonly double ProtonTauMassRatio = 0.528063;
        public static readonly double QuantumOfCirculation = 3.6369475520e-4;
        public static readonly double QuantumOfCirculationTimes2 = 7.2738951040e-4;
        public static readonly double RydbergConstant = 10973731.568539;
        public static readonly double RydbergConstantTimesCInHz = 3.289841960364e15;
        public static readonly double RydbergConstantTimesHcInEv = 13.60569253;
        public static readonly double RydbergConstantTimesHcInJ = 2.179872171e-18;

        public static readonly double ShieldedHelionGyromagneticRatio = 2.037894659e8;
        public static readonly double ShieldedHelionGyromagneticRatioOver2Pi = 32.43410084;
        public static readonly double ShieldedHelionMagneticMoment = -1.074553044e-26;
        public static readonly double ShieldedHelionMagneticMomentToBohrMagnetonRatio = -1.158671471e-3;
        public static readonly double ShieldedHelionMagneticMomentToNuclearMagnetonRatio = -2.127497718;
        public static readonly double ShieldedHelionToProtonMagneticMomentRatio = -0.761766558;
        public static readonly double ShieldedHelionToShieldedProtonMagneticMomentRatio = -0.7617861313;
        public static readonly double ShieldedProtonGyromagneticRatio = 2.675153268e8;
        public static readonly double ShieldedProtonGyromagneticRatioOver2Pi = 42.5763866;
        public static readonly double ShieldedProtonMagneticMoment = 1.410570499e-26;
        public static readonly double ShieldedProtonMagneticMomentToBohrMagnetonRatio = 1.520993128e-3;
        public static readonly double ShieldedProtonMagneticMomentToNuclearMagnetonRatio = 2.792775598;

        public static readonly double TauComptonWavelength = 0.697787e-15;
        public static readonly double TauComptonWavelengthOver2Pi = 0.111056e-15;
        public static readonly double TauElectronMassRatio = 3477.15;
        public static readonly double TauMass = 3.16747e-27;
        public static readonly double TauMassEnergyEquivalent = 2.84678e-10;
        public static readonly double TauMassEnergyEquivalentInMeV = 1776.82;
        public static readonly double TauMassInU = 1.90749;
        public static readonly double TauMolarMass = 1.90749e-3;
        public static readonly double TauMuonMassRatio = 16.8167;
        public static readonly double TauNeutronMassRatio = 1.89111;
        public static readonly double TauProtonMassRatio = 1.89372;
        public static readonly double ThomsonCrossSection = 0.6652458734e-28;

        public static readonly double TritonElectronMassRatio = 5496.9215267;
        public static readonly double TritonGFactor = 5.957924896;
        public static readonly double TritonMagneticMoment = 1.504609447e-26;
        public static readonly double TritonMagneticMomentToBohrMagnetonRatio = 1.622393657e-3;
        public static readonly double TritonMagneticMomentToNuclearMagnetonRatio = 2.978962448;
        public static readonly double TritonMass = 5.00735630e-27;
        public static readonly double TritonMassEnergyEquivalent = 4.50038741e-10;
        public static readonly double TritonMassEnergyEquivalentInMeV = 2808.921005;
        public static readonly double TritonMassInU = 3.0155007134;
        public static readonly double TritonMolarMass = 3.0155007134e-3;
        public static readonly double TritonProtonMassRatio = 2.9937170308;

        public static readonly double WeakMixingAngle = 0.2223;

        #endregion

        #region Physico-chemical constants

        public static readonly double AtomicMassConstant = 1.660538921e-27;
        public static readonly double AtomicMassConstantEnergyEquivalent = 1.492417954e-10;
        public static readonly double AtomicMassConstantEnergyEquivalentInMeV = 931.494061;

        public static readonly double AvogadroConstant = 6.02214129e23;

        public static readonly double BoltzmannConstant = 1.3806488e-23;
        public static readonly double BoltzmannConstantInEvperK = 8.6173324e-5;
        public static readonly double BoltzmannConstantInHzperK = 2.0836618e10;
        public static readonly double BoltzmannConstantInInverseMetersPerKelvin = 69.503476;

        public static readonly double FaradayConstant = 96485.3365;
        public static readonly double FaradayConstantForConventionalElectricCurrent = 96485.3321;

        public static readonly double FirstRadiationConstant = 3.74177153e-16;

        public static readonly double FirstChandrasekharConstant = 0.553960278365090204701121;
        public static readonly double SecondChandrasekharConstant = 0.541926070139289008744561;

        public static readonly double FirstRadiationConstantForSpectralRadiance = 1.191042869e-16;

        public static readonly double LoschmidtConstant27315K_100Kpa = 2.6516462e25;
        public static readonly double LoschmidtConstant27315K_101325Kpa = 2.6867805e25;

        public static readonly double MolarGasConstant = 8.3144621;
        public static readonly double MolarPlanckConstant = 3.9903127176e-10;
        public static readonly double MolarPlanckConstantTimesC = 0.119626565779;
        public static readonly double MolarVolumeOfIdealGas273_15K_100kPa = 22.710953e-3;
        public static readonly double MolarVolumeOfIdealGas273_15K_101325Pa = 22.413968e-3;

        public static readonly double SackurTetrodeConstant1K_100kPa = -1.1517078;
        public static readonly double SackurTetrodeConstant1K_101325Pa = -1.1648708;
        public static readonly double SecondRadiationConstant = 1.4387770e-2;

        public static readonly double StefanBoltzmannConstant = 5.670373e-8;
        public static readonly double WienFrequencyDisplacementLawConstant = 5.8789254e10;
        public static readonly double WienWavelengthDisplacementLawConstant = 2.8977721e-3;

        #endregion

        #region Adopted values

        public static readonly double ConventionalValueOfJosephsonConstant = 483597.9e9;
        public static readonly double ConventionalValueOfVonKlitzingConstant = 25812.807;
        public static readonly double MolarMassConstant = 1e-3;
        public static readonly double MolarMassOfCarbon12 = 12e-3;
        public static readonly double StandardAccelerationOfGravity = 9.80665;
        public static readonly double StandardAtmosphere = 101325;
        public static readonly double StandardStatePressure = 100000;

        #endregion

        #region Non-SI units

        public static readonly double AtomicUnitOf1stHyperpolarizability = 3.206361449e-53;
        public static readonly double AtomicUnitOf2ndHyperpolarizability = 6.23538054e-65;
        public static readonly double AtomicUnitOfAction = 1.054571726e-34;
        public static readonly double AtomicUnitOfCharge = 1.602176565e-19;
        public static readonly double AtomicUnitOfChargeDensity = 1.081202338e12;
        public static readonly double AtomicUnitOfCurrent = 6.62361795e-3;
        public static readonly double AtomicUnitOfElectricDipoleMoment = 8.47835326e-30;
        public static readonly double AtomicUnitOfElectricField = 5.14220652e11;
        public static readonly double AtomicUnitOfElectricFieldGradient = 9.71736200e21;
        public static readonly double AtomicUnitOfElectricPolarizability = 1.6487772754e-41;
        public static readonly double AtomicUnitOfElectricPotential = 27.21138505;
        public static readonly double AtomicUnitOfElectricQuadrupoleMoment = 4.486551331e-40;
        public static readonly double AtomicUnitOfEnergy = 4.35974434e-18;
        public static readonly double AtomicUnitOfForce = 8.23872278e-8;
        public static readonly double AtomicUnitOfLength = 0.52917721092e-10;
        public static readonly double AtomicUnitOfMagneticDipoleMoment = 1.854801936e-23;
        public static readonly double AtomicUnitOfMagneticFluxDensity = 2.350517464e5;
        public static readonly double AtomicUnitOfMagnetizability = 7.891036607e-29;
        public static readonly double AtomicUnitOfMass = 9.10938291e-31;
        public static readonly double AtomicUnitOfMomentum = 1.992851740e-24;
        public static readonly double AtomicUnitOfPermittivity = 1.112650056e-10;
        public static readonly double AtomicUnitOfTime = 2.418884326502e-17;
        public static readonly double AtomicUnitOfVelocity = 2.18769126379e6;

        public static readonly double NaturalUnitOfAction = 1.054571726e-34;
        public static readonly double NaturalUnitOfActionInEvS = 6.58211928e-16;
        public static readonly double NaturalUnitOfEnergy = 8.18710506e-14;
        public static readonly double NaturalUnitOfEnergyInMeV = 0.510998928;
        public static readonly double NaturalUnitOfLength = 386.15926800e-15;
        public static readonly double NaturalUnitOfMass = 9.10938291e-31;
        public static readonly double NaturalUnitOfMomentum = 2.73092429e-22;
        public static readonly double NaturalUnitOfMomentumInMeVperC = 0.510998928;
        public static readonly double NaturalUnitOfTime = 1.28808866833e-21;
        public static readonly double NaturalUnitOfVelocity = 299792458;
        public static readonly double ElectronVolt = 1.602176565e-19;
        public static readonly double PlanckConstantOver2PiTimesCInMeVFm = 197.3269718;

        public static readonly double SpeedOfLightInVacuum = 299792458;
        public static readonly double UnifiedAtomicMassUnit = 1.660538921e-27;

        #endregion

        #region X-ray values

        public static readonly double AngstromStar = 1.00001495e-10;
        public static readonly double CuXUnit = 1.00207697e-13;
        public static readonly double LatticeParameterOfSilicon = 543.1020504e-12;
        public static readonly double MoXUnit = 1.00209952e-13;
        public static readonly double MolarVolumeOfSilicon = 12.05883301e-6;
        public static readonly double _220LatticeSpacingOfSilicon = 192.0155714e-12;

        #endregion

        #region Conversion factors for energy equivalents

        public static readonly double AtomicMassUnitElectronVoltRelationship = 931.494061e6;
        public static readonly double AtomicMassUnitHartreeRelationship = 3.4231776845e7;
        public static readonly double AtomicMassUnitHertzRelationship = 2.2523427168e23;
        public static readonly double AtomicMassUnitInverseMeterRelationship = 7.5130066042e14;
        public static readonly double AtomicMassUnitJouleRelationship = 1.492417954e-10;
        public static readonly double AtomicMassUnitKelvinRelationship = 1.08095408e13;
        public static readonly double AtomicMassUnitKilogramRelationship = 1.660538921e-27;
        public static readonly double HartreeAtomicMassUnitRelationship = 2.9212623246e-8;
        public static readonly double HartreeElectronVoltRelationship = 27.21138505;


        public static readonly double HartreeHertzRelationship = 6.579683920729e15;
        public static readonly double HartreeInverseMeterRelationship = 2.194746313708e7;
        public static readonly double HartreeJouleRelationship = 4.35974434e-18;
        public static readonly double HartreeKelvinRelationship = 3.1577504e5;
        public static readonly double HartreeKilogramRelationship = 4.85086979e-35;


        public static readonly double HertzAtomicMassUnitRelationship = 4.4398216689e-24;
        public static readonly double HertzElectronVoltRelationship = 4.135667516e-15;
        public static readonly double HertzHartreeRelationship = 1.5198298460045e-16;
        public static readonly double HertzInverseMeterRelationship = 3.335640951e-9;
        public static readonly double HertzJouleRelationship = 6.62606957e-34;
        public static readonly double HertzKelvinRelationship = 4.7992434e-11;
        public static readonly double HertzKilogramRelationship = 7.37249668e-51;

        public static readonly double InverseMeterAtomicMassUnitRelationship = 1.33102505120e-15;
        public static readonly double InverseMeterElectronVoltRelationship = 1.239841930e-6;
        public static readonly double InverseMeterHartreeRelationship = 4.556335252755e-8;
        public static readonly double InverseMeterHertzRelationship = 299792458;
        public static readonly double InverseMeterJouleRelationship = 1.986445684e-25;
        public static readonly double InverseMeterKelvinRelationship = 1.4387770e-2;
        public static readonly double InverseMeterKilogramRelationship = 2.210218902e-42;


        public static readonly double JouleAtomicMassUnitRelationship = 6.70053585e9;
        public static readonly double JouleElectronVoltRelationship = 6.24150934e18;
        public static readonly double JouleHartreeRelationship = 2.29371248e17;
        public static readonly double JouleHertzRelationship = 1.509190311e33;
        public static readonly double JouleInverseMeterRelationship = 5.03411701e24;
        public static readonly double JouleKelvinRelationship = 7.2429716e22;
        public static readonly double JouleKilogramRelationship = 1.112650056e-17;
        public static readonly double KelvinAtomicMassUnitRelationship = 9.2510868e-14;
        public static readonly double KelvinElectronVoltRelationship = 8.6173324e-5;
        public static readonly double KelvinHartreeRelationship = 3.1668114e-6;
        public static readonly double KelvinHertzRelationship = 2.0836618e10;
        public static readonly double KelvinInverseMeterRelationship = 69.503476;
        public static readonly double KelvinJouleRelationship = 1.3806488e-23;
        public static readonly double KelvinKilogramRelationship = 1.5361790e-40;
        public static readonly double KilogramAtomicMassUnitRelationship = 6.02214129e26;
        public static readonly double KilogramElectronVoltRelationship = 5.60958885e35;
        public static readonly double KilogramHartreeRelationship = 2.061485968e34;
        public static readonly double KilogramHertzRelationship = 1.356392608e50;
        public static readonly double KilogramInverseMeterRelationship = 4.52443873e41;
        public static readonly double KilogramJouleRelationship = 8.987551787e16;
        public static readonly double KilogramKelvinRelationship = 6.5096582e39;

        #endregion



        #region utils

        public static void parseConstantsFromNIST()
        {
            var sr = new System.IO.StreamReader("allascii.txt");
            var line = "";
            var output = new System.Text.StringBuilder();
            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

            while ((line = sr.ReadLine()) != null)
            {
                var columns = line.Split(new[] { "  " }, System.StringSplitOptions.RemoveEmptyEntries);
                var name =
                    cultureInfo.TextInfo.ToTitleCase(columns[0].Replace("mag.", "magnetic").Replace("mom.", "moment"))
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace(".", "")
                        .Replace("/", "per")
                        .Replace(",", "_")
                        .Replace("{", "_")
                        .Replace("}", "")
                        .Replace(")", "")
                        .Replace("(", "");
                var value = columns[1].Replace(" ", "").Replace("...", "");

                output.AppendLine(string.Format("public static readonly double {0}={1};", name, value));
            }
            sr.Close();
            var sw = new System.IO.StreamWriter("const.txt");
            sw.Write(output);
            sw.Close();
        }

        public const string ToCode =
    @"
          #region Universal

        public static readonly double c = 299792458;

        public static readonly double g = 9.80625;

        public static readonly double h = 6.6260695729*1e-34;

        public static readonly double G = 6.6738480*1e-11;

        public static readonly double MagneticConstant = 4*System.Math.PI*1e-7;

        public static readonly double ElectricConstant = 8.854187817*1e-12;

        public static readonly double ImpedanceOfVacuum = 376.730313461;
        public static readonly double PlanckLength = 1.61619997*1e-35;
        public static readonly double PlanckMass = 2.1765113*1e-8;
        public static readonly double PlanckTemperature = 1.416833e32;
        public static readonly double PlanckTime = 5.3910632*1e-44;

        public static readonly double CharacteristicImpedanceOfVacuum = 376.730313461;

        public static readonly double NewtonianConstantOfGravitation = 6.67384e-11;
        public static readonly double NewtonianConstantOfGravitationOverHBarC = 6.70837e-39;

        public static readonly double PlanckConstant = 6.62606957e-34;
        public static readonly double PlanckConstantInEvS = 4.135667516e-15;
        public static readonly double PlanckConstantOver2Pi = 1.054571726e-34;
        public static readonly double PlanckConstantOver2PiInEvS = 6.58211928e-16;

        public static readonly double PlanckMassEnergyEquivalentInGev = 1.220932e19;

        #endregion

        #region Electromagnetic constants

        public static readonly double BohrMagneton = 927.400968e-26;
        public static readonly double BohrMagnetonInEvperT = 5.7883818066e-5;
        public static readonly double BohrMagnetonInHzperT = 13.99624555e9;
        public static readonly double BohrMagnetonInInverseMetersPerTesla = 46.6864498;
        public static readonly double BohrMagnetonInKperT = 0.67171388;
        public static readonly double ConductanceQuantum = 7.7480917346e-5;
        public static readonly double ElementaryCharge = 1.602176565e-19;
        public static readonly double ElementaryChargeOverH = 2.417989348e14;
        public static readonly double InverseOfConductanceQuantum = 12906.4037217;
        public static readonly double JosephsonConstant = 483597.870e9;
        public static readonly double MagneticFluxQuantum = 2.067833758e-15;
        public static readonly double NuclearMagneton = 5.05078353e-27;
        public static readonly double NuclearMagnetonInEvperT = 3.1524512605e-8;
        public static readonly double NuclearMagnetonInInverseMetersPerTesla = 2.542623527e-2;
        public static readonly double NuclearMagnetonInKperT = 3.6582682e-4;
        public static readonly double NuclearMagnetonInMhzperT = 7.62259357;
        public static readonly double VonKlitzingConstant = 25812.8074434;

        #endregion

        #region Atomic and nuclear constants

        public static readonly double AlphaParticleMass = 6.64465675e-27;
        public static readonly double AlphaParticleMassEnergyEquivalent = 5.97191967e-10;
        public static readonly double AlphaParticleMassEnergyEquivalentInMeV = 3727.379240;
        public static readonly double AlphaParticleMassInU = 4.001506179125;
        public static readonly double AlphaParticleMolarMass = 4.001506179125e-3;
        public static readonly double AlphaParticleElectronMassRatio = 7294.2995361;
        public static readonly double AlphaParticleProtonMassRatio = 3.97259968933;

        public static readonly double BohrRadius = 0.52917721092e-10;

        public static readonly double ClassicalElectronRadius = 2.8179403267e-15;

        public static readonly double ComptonWavelength = 2.4263102389e-12;
        public static readonly double ComptonWavelengthOver2Pi = 386.15926800e-15;

        public static readonly double DeuteronElectronMagneticMomentRatio = -4.664345537e-4;
        public static readonly double DeuteronElectronMassRatio = 3670.4829652;
        public static readonly double DeuteronGFactor = 0.8574382308;
        public static readonly double DeuteronMagneticMoment = 0.433073489e-26;
        public static readonly double DeuteronMagneticMomentToBohrMagnetonRatio = 0.4669754556e-3;
        public static readonly double DeuteronMagneticMomentToNuclearMagnetonRatio = 0.8574382308;
        public static readonly double DeuteronMass = 3.34358348e-27;
        public static readonly double DeuteronMassEnergyEquivalent = 3.00506297e-10;
        public static readonly double DeuteronMassEnergyEquivalentInMeV = 1875.612859;
        public static readonly double DeuteronMassInU = 2.013553212712;
        public static readonly double DeuteronMolarMass = 2.013553212712e-3;
        public static readonly double DeuteronNeutronMagneticMomentRatio = -0.44820652;
        public static readonly double DeuteronProtonMagneticMomentRatio = 0.3070122070;
        public static readonly double DeuteronProtonMassRatio = 1.99900750097;
        public static readonly double DeuteronRmsChargeRadius = 2.1424e-15;

        public static readonly double ElectronChargeToMassQuotient = -1.758820088e11;
        public static readonly double ElectronDeuteronMagneticMomentRatio = -2143.923498;
        public static readonly double ElectronDeuteronMassRatio = 2.7244371095e-4;
        public static readonly double ElectronGFactor = -2.00231930436153;
        public static readonly double ElectronGyromagneticRatio = 1.760859708e11;
        public static readonly double ElectronGyromagneticRatioOver2Pi = 28024.95266;
        public static readonly double ElectronHelionMassRatio = 1.8195430761e-4;
        public static readonly double ElectronMagneticMoment = -928.476430e-26;
        public static readonly double ElectronMagneticMomentAnomaly = 1.15965218076e-3;
        public static readonly double ElectronMagneticMomentToBohrMagnetonRatio = -1.00115965218076;
        public static readonly double ElectronMagneticMomentToNuclearMagnetonRatio = -1838.28197090;
        public static readonly double ElectronMass = 9.10938291e-31;
        public static readonly double ElectronMassEnergyEquivalent = 8.18710506e-14;
        public static readonly double ElectronMassEnergyEquivalentInMeV = 0.510998928;
        public static readonly double ElectronMassInU = 5.4857990946e-4;
        public static readonly double ElectronMolarMass = 5.4857990946e-7;
        public static readonly double ElectronMuonMagneticMomentRatio = 206.7669896;
        public static readonly double ElectronMuonMassRatio = 4.83633166e-3;
        public static readonly double ElectronNeutronMagneticMomentRatio = 960.92050;
        public static readonly double ElectronNeutronMassRatio = 5.4386734461e-4;
        public static readonly double ElectronProtonMagneticMomentRatio = -658.2106848;
        public static readonly double ElectronProtonMassRatio = 5.4461702178e-4;
        public static readonly double ElectronTauMassRatio = 2.87592e-4;
        public static readonly double ElectronToAlphaParticleMassRatio = 1.37093355578e-4;
        public static readonly double ElectronToShieldedHelionMagneticMomentRatio = 864.058257;
        public static readonly double ElectronToShieldedProtonMagneticMomentRatio = -658.2275971;
        public static readonly double ElectronTritonMassRatio = 1.8192000653e-4;
        public static readonly double ElectronVoltAtomicMassUnitRelationship = 1.073544150e-9;
        public static readonly double ElectronVoltHartreeRelationship = 3.674932379e-2;
        public static readonly double ElectronVoltHertzRelationship = 2.417989348e14;
        public static readonly double ElectronVoltInverseMeterRelationship = 8.06554429e5;
        public static readonly double ElectronVoltJouleRelationship = 1.602176565e-19;
        public static readonly double ElectronVoltKelvinRelationship = 1.1604519e4;
        public static readonly double ElectronVoltKilogramRelationship = 1.782661845e-36;

        public static readonly double FermiCouplingConstant = 1.166364e-5;
        public static readonly double FineStructureConstant = 7.2973525698e-3;

        public static readonly double HartreeEnergy = 4.35974434e-18;
        public static readonly double HartreeEnergyInEv = 27.21138505;

        public static readonly double HelionElectronMassRatio = 5495.8852754;
        public static readonly double HelionGFactor = -4.255250613;
        public static readonly double HelionMagneticMoment = -1.074617486e-26;
        public static readonly double HelionMagneticMomentToBohrMagnetonRatio = -1.158740958e-3;
        public static readonly double HelionMagneticMomentToNuclearMagnetonRatio = -2.127625306;
        public static readonly double HelionMass = 5.00641234e-27;
        public static readonly double HelionMassEnergyEquivalent = 4.49953902e-10;
        public static readonly double HelionMassEnergyEquivalentInMeV = 2808.391482;
        public static readonly double HelionMassInU = 3.0149322468;
        public static readonly double HelionMolarMass = 3.0149322468e-3;
        public static readonly double HelionProtonMassRatio = 2.9931526707;

        public static readonly double InverseFineStructureConstant = 137.035999074;

        public static readonly double MuonComptonWavelength = 11.73444103e-15;
        public static readonly double MuonComptonWavelengthOver2Pi = 1.867594294e-15;
        public static readonly double MuonElectronMassRatio = 206.7682843;
        public static readonly double MuonGFactor = -2.0023318418;
        public static readonly double MuonMagneticMoment = -4.49044807e-26;
        public static readonly double MuonMagneticMomentAnomaly = 1.16592091e-3;
        public static readonly double MuonMagneticMomentToBohrMagnetonRatio = -4.84197044e-3;
        public static readonly double MuonMagneticMomentToNuclearMagnetonRatio = -8.89059697;
        public static readonly double MuonMass = 1.883531475e-28;
        public static readonly double MuonMassEnergyEquivalent = 1.692833667e-11;
        public static readonly double MuonMassEnergyEquivalentInMeV = 105.6583715;
        public static readonly double MuonMassInU = 0.1134289267;
        public static readonly double MuonMolarMass = 0.1134289267e-3;
        public static readonly double MuonNeutronMassRatio = 0.1124545177;
        public static readonly double MuonProtonMagneticMomentRatio = -3.183345107;
        public static readonly double MuonProtonMassRatio = 0.1126095272;
        public static readonly double MuonTauMassRatio = 5.94649e-2;

        public static readonly double NeutronComptonWavelength = 1.3195909068e-15;
        public static readonly double NeutronComptonWavelengthOver2Pi = 0.21001941568e-15;
        public static readonly double NeutronElectronMagneticMomentRatio = 1.04066882e-3;
        public static readonly double NeutronElectronMassRatio = 1838.6836605;
        public static readonly double NeutronGFactor = -3.82608545;
        public static readonly double NeutronGyromagneticRatio = 1.83247179e8;
        public static readonly double NeutronGyromagneticRatioOver2Pi = 29.1646943;
        public static readonly double NeutronMagneticMoment = -0.96623647e-26;
        public static readonly double NeutronMagneticMomentToBohrMagnetonRatio = -1.04187563e-3;
        public static readonly double NeutronMagneticMomentToNuclearMagnetonRatio = -1.91304272;
        public static readonly double NeutronMass = 1.674927351e-27;
        public static readonly double NeutronMassEnergyEquivalent = 1.505349631e-10;
        public static readonly double NeutronMassEnergyEquivalentInMeV = 939.565379;
        public static readonly double NeutronMassInU = 1.00866491600;
        public static readonly double NeutronMolarMass = 1.00866491600e-3;
        public static readonly double NeutronMuonMassRatio = 8.89248400;
        public static readonly double NeutronProtonMagneticMomentRatio = -0.68497934;
        public static readonly double NeutronProtonMassDifference = 2.30557392e-30;
        public static readonly double NeutronProtonMassDifferenceEnergyEquivalent = 2.07214650e-13;
        public static readonly double NeutronProtonMassDifferenceEnergyEquivalentInMeV = 1.29333217;
        public static readonly double NeutronProtonMassDifferenceInU = 0.00138844919;
        public static readonly double NeutronProtonMassRatio = 1.00137841917;
        public static readonly double NeutronTauMassRatio = 0.528790;
        public static readonly double NeutronToShieldedProtonMagneticMomentRatio = -0.68499694;

        public static readonly double ProtonChargeToMassQuotient = 9.57883358e7;
        public static readonly double ProtonComptonWavelength = 1.32140985623e-15;
        public static readonly double ProtonComptonWavelengthOver2Pi = 0.21030891047e-15;
        public static readonly double ProtonElectronMassRatio = 1836.15267245;
        public static readonly double ProtonGFactor = 5.585694713;
        public static readonly double ProtonGyromagneticRatio = 2.675222005e8;
        public static readonly double ProtonGyromagneticRatioOver2Pi = 42.5774806;
        public static readonly double ProtonMagneticMoment = 1.410606743e-26;
        public static readonly double ProtonMagneticMomentToBohrMagnetonRatio = 1.521032210e-3;
        public static readonly double ProtonMagneticMomentToNuclearMagnetonRatio = 2.792847356;
        public static readonly double ProtonMagneticShieldingCorrection = 25.694e-6;
        public static readonly double ProtonMass = 1.672621777e-27;
        public static readonly double ProtonMassEnergyEquivalent = 1.503277484e-10;
        public static readonly double ProtonMassEnergyEquivalentInMeV = 938.272046;
        public static readonly double ProtonMassInU = 1.007276466812;
        public static readonly double ProtonMolarMass = 1.007276466812e-3;
        public static readonly double ProtonMuonMassRatio = 8.88024331;
        public static readonly double ProtonNeutronMagneticMomentRatio = -1.45989806;
        public static readonly double ProtonNeutronMassRatio = 0.99862347826;
        public static readonly double ProtonRmsChargeRadius = 0.8775e-15;
        public static readonly double ProtonTauMassRatio = 0.528063;
        public static readonly double QuantumOfCirculation = 3.6369475520e-4;
        public static readonly double QuantumOfCirculationTimes2 = 7.2738951040e-4;
        public static readonly double RydbergConstant = 10973731.568539;
        public static readonly double RydbergConstantTimesCInHz = 3.289841960364e15;
        public static readonly double RydbergConstantTimesHcInEv = 13.60569253;
        public static readonly double RydbergConstantTimesHcInJ = 2.179872171e-18;

        public static readonly double ShieldedHelionGyromagneticRatio = 2.037894659e8;
        public static readonly double ShieldedHelionGyromagneticRatioOver2Pi = 32.43410084;
        public static readonly double ShieldedHelionMagneticMoment = -1.074553044e-26;
        public static readonly double ShieldedHelionMagneticMomentToBohrMagnetonRatio = -1.158671471e-3;
        public static readonly double ShieldedHelionMagneticMomentToNuclearMagnetonRatio = -2.127497718;
        public static readonly double ShieldedHelionToProtonMagneticMomentRatio = -0.761766558;
        public static readonly double ShieldedHelionToShieldedProtonMagneticMomentRatio = -0.7617861313;
        public static readonly double ShieldedProtonGyromagneticRatio = 2.675153268e8;
        public static readonly double ShieldedProtonGyromagneticRatioOver2Pi = 42.5763866;
        public static readonly double ShieldedProtonMagneticMoment = 1.410570499e-26;
        public static readonly double ShieldedProtonMagneticMomentToBohrMagnetonRatio = 1.520993128e-3;
        public static readonly double ShieldedProtonMagneticMomentToNuclearMagnetonRatio = 2.792775598;

        public static readonly double TauComptonWavelength = 0.697787e-15;
        public static readonly double TauComptonWavelengthOver2Pi = 0.111056e-15;
        public static readonly double TauElectronMassRatio = 3477.15;
        public static readonly double TauMass = 3.16747e-27;
        public static readonly double TauMassEnergyEquivalent = 2.84678e-10;
        public static readonly double TauMassEnergyEquivalentInMeV = 1776.82;
        public static readonly double TauMassInU = 1.90749;
        public static readonly double TauMolarMass = 1.90749e-3;
        public static readonly double TauMuonMassRatio = 16.8167;
        public static readonly double TauNeutronMassRatio = 1.89111;
        public static readonly double TauProtonMassRatio = 1.89372;
        public static readonly double ThomsonCrossSection = 0.6652458734e-28;

        public static readonly double TritonElectronMassRatio = 5496.9215267;
        public static readonly double TritonGFactor = 5.957924896;
        public static readonly double TritonMagneticMoment = 1.504609447e-26;
        public static readonly double TritonMagneticMomentToBohrMagnetonRatio = 1.622393657e-3;
        public static readonly double TritonMagneticMomentToNuclearMagnetonRatio = 2.978962448;
        public static readonly double TritonMass = 5.00735630e-27;
        public static readonly double TritonMassEnergyEquivalent = 4.50038741e-10;
        public static readonly double TritonMassEnergyEquivalentInMeV = 2808.921005;
        public static readonly double TritonMassInU = 3.0155007134;
        public static readonly double TritonMolarMass = 3.0155007134e-3;
        public static readonly double TritonProtonMassRatio = 2.9937170308;

        public static readonly double WeakMixingAngle = 0.2223;

        #endregion

        #region Physico-chemical constants

        public static readonly double AtomicMassConstant = 1.660538921e-27;
        public static readonly double AtomicMassConstantEnergyEquivalent = 1.492417954e-10;
        public static readonly double AtomicMassConstantEnergyEquivalentInMeV = 931.494061;

        public static readonly double AvogadroConstant = 6.02214129e23;

        public static readonly double BoltzmannConstant = 1.3806488e-23;
        public static readonly double BoltzmannConstantInEvperK = 8.6173324e-5;
        public static readonly double BoltzmannConstantInHzperK = 2.0836618e10;
        public static readonly double BoltzmannConstantInInverseMetersPerKelvin = 69.503476;

        public static readonly double FaradayConstant = 96485.3365;
        public static readonly double FaradayConstantForConventionalElectricCurrent = 96485.3321;

        public static readonly double FirstRadiationConstant = 3.74177153e-16;

        public static readonly double FirstChandrasekharConstant = 0.553960278365090204701121;
        public static readonly double SecondChandrasekharConstant = 0.541926070139289008744561;

        public static readonly double FirstRadiationConstantForSpectralRadiance = 1.191042869e-16;

        public static readonly double LoschmidtConstant27315K_100Kpa = 2.6516462e25;
        public static readonly double LoschmidtConstant27315K_101325Kpa = 2.6867805e25;

        public static readonly double MolarGasConstant = 8.3144621;
        public static readonly double MolarPlanckConstant = 3.9903127176e-10;
        public static readonly double MolarPlanckConstantTimesC = 0.119626565779;
        public static readonly double MolarVolumeOfIdealGas273_15K_100kPa = 22.710953e-3;
        public static readonly double MolarVolumeOfIdealGas273_15K_101325Pa = 22.413968e-3;

        public static readonly double SackurTetrodeConstant1K_100kPa = -1.1517078;
        public static readonly double SackurTetrodeConstant1K_101325Pa = -1.1648708;
        public static readonly double SecondRadiationConstant = 1.4387770e-2;

        public static readonly double StefanBoltzmannConstant = 5.670373e-8;
        public static readonly double WienFrequencyDisplacementLawConstant = 5.8789254e10;
        public static readonly double WienWavelengthDisplacementLawConstant = 2.8977721e-3;

        #endregion

        #region Adopted values

        public static readonly double ConventionalValueOfJosephsonConstant = 483597.9e9;
        public static readonly double ConventionalValueOfVonKlitzingConstant = 25812.807;
        public static readonly double MolarMassConstant = 1e-3;
        public static readonly double MolarMassOfCarbon12 = 12e-3;
        public static readonly double StandardAccelerationOfGravity = 9.80665;
        public static readonly double StandardAtmosphere = 101325;
        public static readonly double StandardStatePressure = 100000;

        #endregion

        #region Non-SI units

        public static readonly double AtomicUnitOf1stHyperpolarizability = 3.206361449e-53;
        public static readonly double AtomicUnitOf2ndHyperpolarizability = 6.23538054e-65;
        public static readonly double AtomicUnitOfAction = 1.054571726e-34;
        public static readonly double AtomicUnitOfCharge = 1.602176565e-19;
        public static readonly double AtomicUnitOfChargeDensity = 1.081202338e12;
        public static readonly double AtomicUnitOfCurrent = 6.62361795e-3;
        public static readonly double AtomicUnitOfElectricDipoleMoment = 8.47835326e-30;
        public static readonly double AtomicUnitOfElectricField = 5.14220652e11;
        public static readonly double AtomicUnitOfElectricFieldGradient = 9.71736200e21;
        public static readonly double AtomicUnitOfElectricPolarizability = 1.6487772754e-41;
        public static readonly double AtomicUnitOfElectricPotential = 27.21138505;
        public static readonly double AtomicUnitOfElectricQuadrupoleMoment = 4.486551331e-40;
        public static readonly double AtomicUnitOfEnergy = 4.35974434e-18;
        public static readonly double AtomicUnitOfForce = 8.23872278e-8;
        public static readonly double AtomicUnitOfLength = 0.52917721092e-10;
        public static readonly double AtomicUnitOfMagneticDipoleMoment = 1.854801936e-23;
        public static readonly double AtomicUnitOfMagneticFluxDensity = 2.350517464e5;
        public static readonly double AtomicUnitOfMagnetizability = 7.891036607e-29;
        public static readonly double AtomicUnitOfMass = 9.10938291e-31;
        public static readonly double AtomicUnitOfMomentum = 1.992851740e-24;
        public static readonly double AtomicUnitOfPermittivity = 1.112650056e-10;
        public static readonly double AtomicUnitOfTime = 2.418884326502e-17;
        public static readonly double AtomicUnitOfVelocity = 2.18769126379e6;

        public static readonly double NaturalUnitOfAction = 1.054571726e-34;
        public static readonly double NaturalUnitOfActionInEvS = 6.58211928e-16;
        public static readonly double NaturalUnitOfEnergy = 8.18710506e-14;
        public static readonly double NaturalUnitOfEnergyInMeV = 0.510998928;
        public static readonly double NaturalUnitOfLength = 386.15926800e-15;
        public static readonly double NaturalUnitOfMass = 9.10938291e-31;
        public static readonly double NaturalUnitOfMomentum = 2.73092429e-22;
        public static readonly double NaturalUnitOfMomentumInMeVperC = 0.510998928;
        public static readonly double NaturalUnitOfTime = 1.28808866833e-21;
        public static readonly double NaturalUnitOfVelocity = 299792458;
        public static readonly double ElectronVolt = 1.602176565e-19;
        public static readonly double PlanckConstantOver2PiTimesCInMeVFm = 197.3269718;

        public static readonly double SpeedOfLightInVacuum = 299792458;
        public static readonly double UnifiedAtomicMassUnit = 1.660538921e-27;

        #endregion

        #region X-ray values

        public static readonly double AngstromStar = 1.00001495e-10;
        public static readonly double CuXUnit = 1.00207697e-13;
        public static readonly double LatticeParameterOfSilicon = 543.1020504e-12;
        public static readonly double MoXUnit = 1.00209952e-13;
        public static readonly double MolarVolumeOfSilicon = 12.05883301e-6;
        public static readonly double _220LatticeSpacingOfSilicon = 192.0155714e-12;

        #endregion

        #region Conversion factors for energy equivalents

        public static readonly double AtomicMassUnitElectronVoltRelationship = 931.494061e6;
        public static readonly double AtomicMassUnitHartreeRelationship = 3.4231776845e7;
        public static readonly double AtomicMassUnitHertzRelationship = 2.2523427168e23;
        public static readonly double AtomicMassUnitInverseMeterRelationship = 7.5130066042e14;
        public static readonly double AtomicMassUnitJouleRelationship = 1.492417954e-10;
        public static readonly double AtomicMassUnitKelvinRelationship = 1.08095408e13;
        public static readonly double AtomicMassUnitKilogramRelationship = 1.660538921e-27;
        public static readonly double HartreeAtomicMassUnitRelationship = 2.9212623246e-8;
        public static readonly double HartreeElectronVoltRelationship = 27.21138505;


        public static readonly double HartreeHertzRelationship = 6.579683920729e15;
        public static readonly double HartreeInverseMeterRelationship = 2.194746313708e7;
        public static readonly double HartreeJouleRelationship = 4.35974434e-18;
        public static readonly double HartreeKelvinRelationship = 3.1577504e5;
        public static readonly double HartreeKilogramRelationship = 4.85086979e-35;


        public static readonly double HertzAtomicMassUnitRelationship = 4.4398216689e-24;
        public static readonly double HertzElectronVoltRelationship = 4.135667516e-15;
        public static readonly double HertzHartreeRelationship = 1.5198298460045e-16;
        public static readonly double HertzInverseMeterRelationship = 3.335640951e-9;
        public static readonly double HertzJouleRelationship = 6.62606957e-34;
        public static readonly double HertzKelvinRelationship = 4.7992434e-11;
        public static readonly double HertzKilogramRelationship = 7.37249668e-51;

        public static readonly double InverseMeterAtomicMassUnitRelationship = 1.33102505120e-15;
        public static readonly double InverseMeterElectronVoltRelationship = 1.239841930e-6;
        public static readonly double InverseMeterHartreeRelationship = 4.556335252755e-8;
        public static readonly double InverseMeterHertzRelationship = 299792458;
        public static readonly double InverseMeterJouleRelationship = 1.986445684e-25;
        public static readonly double InverseMeterKelvinRelationship = 1.4387770e-2;
        public static readonly double InverseMeterKilogramRelationship = 2.210218902e-42;


        public static readonly double JouleAtomicMassUnitRelationship = 6.70053585e9;
        public static readonly double JouleElectronVoltRelationship = 6.24150934e18;
        public static readonly double JouleHartreeRelationship = 2.29371248e17;
        public static readonly double JouleHertzRelationship = 1.509190311e33;
        public static readonly double JouleInverseMeterRelationship = 5.03411701e24;
        public static readonly double JouleKelvinRelationship = 7.2429716e22;
        public static readonly double JouleKilogramRelationship = 1.112650056e-17;
        public static readonly double KelvinAtomicMassUnitRelationship = 9.2510868e-14;
        public static readonly double KelvinElectronVoltRelationship = 8.6173324e-5;
        public static readonly double KelvinHartreeRelationship = 3.1668114e-6;
        public static readonly double KelvinHertzRelationship = 2.0836618e10;
        public static readonly double KelvinInverseMeterRelationship = 69.503476;
        public static readonly double KelvinJouleRelationship = 1.3806488e-23;
        public static readonly double KelvinKilogramRelationship = 1.5361790e-40;
        public static readonly double KilogramAtomicMassUnitRelationship = 6.02214129e26;
        public static readonly double KilogramElectronVoltRelationship = 5.60958885e35;
        public static readonly double KilogramHartreeRelationship = 2.061485968e34;
        public static readonly double KilogramHertzRelationship = 1.356392608e50;
        public static readonly double KilogramInverseMeterRelationship = 4.52443873e41;
        public static readonly double KilogramJouleRelationship = 8.987551787e16;
        public static readonly double KilogramKelvinRelationship = 6.5096582e39;

        #endregion

";

        #endregion
    }
}