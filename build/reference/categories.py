"""Reviewed subjects for the built-in reference and its menus.

Function names are canonical: the elementary/special authoring modules resolve
aliases before lookup. The established function groups come from the pre-#172
catalogue; constant groups preserve their engine CategoryAttribute metadata.
New names require an explicit subject choice, never a runtime-type fallback.
"""

GROUPS = {
    "ElementaryFunctions": {
        "Trigonometric functions": (
            "cos", "cot", "csc", "sec", "sin", "sinc", "tan",
        ),
        "Hyperbolic trigonometric functions": (
            "cosh", "coth", "csch", "sech", "sinh", "tanh",
        ),
        "Inverse trigonometric functions": (
            "arccos", "arccot", "arccsc", "arcsec", "arcsin", "arctan",
        ),
        "Inverse hyperbolic functions": (
            "arcosh", "arcoth", "arcsch", "arsech", "arsinh", "artanh",
        ),
        "Logarithms": (
            "ln", "log", "log10", "log2",
        ),
        "Root functions": (
            "root", "sqrt",
        ),
        "Exponentiation": (
            "exp", "pow",
        ),
        "Coefficients": (
            "BinomialCoeff", "TaylorCoeff",
        ),
        "Combinatorics": (
            "Combinations", "CombinationsWithRepetition", "Permutations", "Variations",
            "VariationsWithRepetition",
        ),
        "Integer functions": (
            "DoubleFactorial", "Eulerφ", "factorial", "gcd", "isPrime", "lcm", "prime",
        ),
        "Complex specific functions": (
            "Im", "Magnitude", "Phase", "Re", "conj",
        ),
        "Step functions": (
            "DiracDelta", "Heaviside", "KroneckerDelta", "sgn",
        ),
        "Absolute value": (
            "abs",
        ),
    },
    "SpecialFunctions": {
        "Mathieu functions": (
            "MathieuAn", "MathieuBn", "MathieuCE", "MathieuMc", "MathieuMs", "MathieuSE",
        ),
        "Elliptic integrals": (
            "CarlsonC", "CarlsonD", "CarlsonF", "CarlsonJ", "EllipticD", "EllipticE", "EllipticF",
            "EllipticK", "EllipticΠ", "JacobiEllipticCn", "JacobiEllipticDn", "JacobiEllipticSn",
        ),
        "Functions related to probability distributions": (
            "OwenT", "erf", "erfQ", "erfZ", "erfc", "erfcx", "erfi", "faddeeva", "hazard", "inverseErf",
            "inverseErfc", "logErfc",
        ),
        "Rotational symmetry": (
            "ClebschGordan", "Coupling3j", "Coupling6j", "Coupling9j", "CouplingRacahW",
            "SphericalHarmonic",
        ),
        "Hypergeometric functions": (
            "Hypergeometric0F1", "Hypergeometric1F1", "Hypergeometric2F0", "Hypergeometric2F1",
            "Hypergeometric2F1renorm", "HypergeometricU",
        ),
        "Orthogonal polynomials": (
            "ChebyshevT", "ConicalP", "CylindricalConicalP", "Gegenbauer", "Gegenbauer1", "Gegenbauer2",
            "Gegenbauer3", "HermiteH", "HermiteHe", "Laguerre", "LegendreH3D", "LegendreP", "LegendreQ",
            "SphericalConicalP", "SphericalLegendreP", "ZernikeR",
        ),
        "Optimization benchmarks": (
            "Ackley", "Bohachevsky1", "Himmelblau", "Matyas", "Rastrigin", "Rosenbrock", "dropWave",
            "sixHumpCamel",
        ),
        "Gamma and related functions": (
            "Beta", "BetaNorm", "Pochhammer", "PochhammerRelative", "gamma", "gammaP", "gammaQ",
            "leftRegularizedGamma", "logBeta", "logGamma", "polyGamma", "psi", "rightRegularizedGamma",
        ),
        "Coulomb functions": (
            "CoulombC", "CoulombF", "CoulombFprime", "CoulombG", "CoulombGprime", "CoulombW",
            "HydrogenicR",
        ),
        "Lambert functions": (
            "LambertW0", "LambertWm1",
        ),
        "Integral functions": (
            "Chi", "Ci", "Clausen", "Dawson", "Debye", "Ei", "Ein", "En", "FermiDiracF0",
            "FermiDiracF3half", "FermiDiracFhalf", "FermiDiracFj", "FermiDiracFmhalf", "Fresnel",
            "FresnelC", "FresnelS", "Shi", "Si", "SynchrotronF", "SynchrotronG", "Tai", "Ti", "TransportJ",
        ),
        "Bessel functions": (
            "BesselJ0", "BesselJ1", "BesselJ2", "BesselJ3", "BesselJn", "BesselJnPrime", "BesselJν",
            "BesselJνZeros", "BesselY0", "BesselY1", "BesselY2", "BesselY3", "BesselYn", "BesselYν",
            "Hankel1", "Hankel2", "ModifiedBesselIn", "ModifiedBesselIν", "ModifiedBesselKn",
            "ModifiedBesselKν", "ModifiedSphericalBesselIn", "ModifiedSphericalBesselKn", "RiccatiBesselC",
            "RiccatiBesselS", "RiccatiBesselζ", "RiccatiBesselξ", "SphericalBesselJ0", "SphericalBesselJ1",
            "SphericalBesselJ2", "SphericalBesselJ3", "SphericalBesselJn", "SphericalBesselY0",
            "SphericalBesselY1", "SphericalBesselY2", "SphericalBesselY3", "SphericalBesselYn",
            "SphericalHankel1", "SphericalHankel2", "StruveL0", "StruveL1", "logBesselKν",
        ),
        "Airy functions": (
            "Ai", "AiPrime", "AiZeros", "AiZerosPrime", "Bi", "BiPrime", "BiZeros", "BiZerosPrime",
        ),
        "Zeta and L-functions": (
            "DirichletEta", "HurwitzZeta", "PolyLog", "RiemannZeta", "SpencesIntegral", "diLog",
        ),
        "Logistic functions": (
            "Logistic", "Logit",
        ),
        "Root finding": (
            "findRoot",
        ),
        "Signal processing": (
            "Gabor",
        ),
    },
    "PhysicalConstants": {
        "Universal": (
            "c", "g", "h", "G", "MagneticConstant", "ElectricConstant", "ImpedanceOfVacuum",
            "PlanckLength", "PlanckMass", "PlanckTemperature", "PlanckTime",
            "CharacteristicImpedanceOfVacuum", "NewtonianConstantOfGravitation",
            "NewtonianConstantOfGravitationOverHBarC", "PlanckConstant", "PlanckConstantInEvS",
            "PlanckConstantOver2Pi", "PlanckConstantOver2PiInEvS", "PlanckMassEnergyEquivalentInGev",
        ),
        "Electromagnetic constants": (
            "BohrMagneton", "BohrMagnetonInEvperT", "BohrMagnetonInHzperT",
            "BohrMagnetonInInverseMetersPerTesla", "BohrMagnetonInKperT", "ConductanceQuantum",
            "ElementaryCharge", "ElementaryChargeOverH", "InverseOfConductanceQuantum",
            "JosephsonConstant", "MagneticFluxQuantum", "NuclearMagneton", "NuclearMagnetonInEvperT",
            "NuclearMagnetonInInverseMetersPerTesla", "NuclearMagnetonInKperT", "NuclearMagnetonInMhzperT",
            "VonKlitzingConstant",
        ),
        "Atomic and nuclear constants": (
            "AlphaParticleMass", "AlphaParticleMassEnergyEquivalent",
            "AlphaParticleMassEnergyEquivalentInMeV", "AlphaParticleMassInU", "AlphaParticleMolarMass",
            "AlphaParticleElectronMassRatio", "AlphaParticleProtonMassRatio", "BohrRadius",
            "ClassicalElectronRadius", "ComptonWavelength", "ComptonWavelengthOver2Pi",
            "DeuteronElectronMagneticMomentRatio", "DeuteronElectronMassRatio", "DeuteronGFactor",
            "DeuteronMagneticMoment", "DeuteronMagneticMomentToBohrMagnetonRatio",
            "DeuteronMagneticMomentToNuclearMagnetonRatio", "DeuteronMass", "DeuteronMassEnergyEquivalent",
            "DeuteronMassEnergyEquivalentInMeV", "DeuteronMassInU", "DeuteronMolarMass",
            "DeuteronNeutronMagneticMomentRatio", "DeuteronProtonMagneticMomentRatio",
            "DeuteronProtonMassRatio", "DeuteronRmsChargeRadius", "ElectronChargeToMassQuotient",
            "ElectronDeuteronMagneticMomentRatio", "ElectronDeuteronMassRatio", "ElectronGFactor",
            "ElectronGyromagneticRatio", "ElectronGyromagneticRatioOver2Pi", "ElectronHelionMassRatio",
            "ElectronMagneticMoment", "ElectronMagneticMomentAnomaly",
            "ElectronMagneticMomentToBohrMagnetonRatio", "ElectronMagneticMomentToNuclearMagnetonRatio",
            "ElectronMass", "ElectronMassEnergyEquivalent", "ElectronMassEnergyEquivalentInMeV",
            "ElectronMassInU", "ElectronMolarMass", "ElectronMuonMagneticMomentRatio",
            "ElectronMuonMassRatio", "ElectronNeutronMagneticMomentRatio", "ElectronNeutronMassRatio",
            "ElectronProtonMagneticMomentRatio", "ElectronProtonMassRatio", "ElectronTauMassRatio",
            "ElectronToAlphaParticleMassRatio", "ElectronToShieldedHelionMagneticMomentRatio",
            "ElectronToShieldedProtonMagneticMomentRatio", "ElectronTritonMassRatio",
            "ElectronVoltAtomicMassUnitRelationship", "ElectronVoltHartreeRelationship",
            "ElectronVoltHertzRelationship", "ElectronVoltInverseMeterRelationship",
            "ElectronVoltJouleRelationship", "ElectronVoltKelvinRelationship",
            "ElectronVoltKilogramRelationship", "FermiCouplingConstant", "FineStructureConstant",
            "HartreeEnergy", "HartreeEnergyInEv", "HelionElectronMassRatio", "HelionGFactor",
            "HelionMagneticMoment", "HelionMagneticMomentToBohrMagnetonRatio",
            "HelionMagneticMomentToNuclearMagnetonRatio", "HelionMass", "HelionMassEnergyEquivalent",
            "HelionMassEnergyEquivalentInMeV", "HelionMassInU", "HelionMolarMass", "HelionProtonMassRatio",
            "InverseFineStructureConstant", "MuonComptonWavelength", "MuonComptonWavelengthOver2Pi",
            "MuonElectronMassRatio", "MuonGFactor", "MuonMagneticMoment", "MuonMagneticMomentAnomaly",
            "MuonMagneticMomentToBohrMagnetonRatio", "MuonMagneticMomentToNuclearMagnetonRatio",
            "MuonMass", "MuonMassEnergyEquivalent", "MuonMassEnergyEquivalentInMeV", "MuonMassInU",
            "MuonMolarMass", "MuonNeutronMassRatio", "MuonProtonMagneticMomentRatio",
            "MuonProtonMassRatio", "MuonTauMassRatio", "NeutronComptonWavelength",
            "NeutronComptonWavelengthOver2Pi", "NeutronElectronMagneticMomentRatio",
            "NeutronElectronMassRatio", "NeutronGFactor", "NeutronGyromagneticRatio",
            "NeutronGyromagneticRatioOver2Pi", "NeutronMagneticMoment",
            "NeutronMagneticMomentToBohrMagnetonRatio", "NeutronMagneticMomentToNuclearMagnetonRatio",
            "NeutronMass", "NeutronMassEnergyEquivalent", "NeutronMassEnergyEquivalentInMeV",
            "NeutronMassInU", "NeutronMolarMass", "NeutronMuonMassRatio",
            "NeutronProtonMagneticMomentRatio", "NeutronProtonMassDifference",
            "NeutronProtonMassDifferenceEnergyEquivalent",
            "NeutronProtonMassDifferenceEnergyEquivalentInMeV", "NeutronProtonMassDifferenceInU",
            "NeutronProtonMassRatio", "NeutronTauMassRatio", "NeutronToShieldedProtonMagneticMomentRatio",
            "ProtonChargeToMassQuotient", "ProtonComptonWavelength", "ProtonComptonWavelengthOver2Pi",
            "ProtonElectronMassRatio", "ProtonGFactor", "ProtonGyromagneticRatio",
            "ProtonGyromagneticRatioOver2Pi", "ProtonMagneticMoment",
            "ProtonMagneticMomentToBohrMagnetonRatio", "ProtonMagneticMomentToNuclearMagnetonRatio",
            "ProtonMagneticShieldingCorrection", "ProtonMass", "ProtonMassEnergyEquivalent",
            "ProtonMassEnergyEquivalentInMeV", "ProtonMassInU", "ProtonMolarMass", "ProtonMuonMassRatio",
            "ProtonNeutronMagneticMomentRatio", "ProtonNeutronMassRatio", "ProtonRmsChargeRadius",
            "ProtonTauMassRatio", "QuantumOfCirculation", "QuantumOfCirculationTimes2", "RydbergConstant",
            "RydbergConstantTimesCInHz", "RydbergConstantTimesHcInEv", "RydbergConstantTimesHcInJ",
            "ShieldedHelionGyromagneticRatio", "ShieldedHelionGyromagneticRatioOver2Pi",
            "ShieldedHelionMagneticMoment", "ShieldedHelionMagneticMomentToBohrMagnetonRatio",
            "ShieldedHelionMagneticMomentToNuclearMagnetonRatio",
            "ShieldedHelionToProtonMagneticMomentRatio",
            "ShieldedHelionToShieldedProtonMagneticMomentRatio", "ShieldedProtonGyromagneticRatio",
            "ShieldedProtonGyromagneticRatioOver2Pi", "ShieldedProtonMagneticMoment",
            "ShieldedProtonMagneticMomentToBohrMagnetonRatio",
            "ShieldedProtonMagneticMomentToNuclearMagnetonRatio", "TauComptonWavelength",
            "TauComptonWavelengthOver2Pi", "TauElectronMassRatio", "TauMass", "TauMassEnergyEquivalent",
            "TauMassEnergyEquivalentInMeV", "TauMassInU", "TauMolarMass", "TauMuonMassRatio",
            "TauNeutronMassRatio", "TauProtonMassRatio", "ThomsonCrossSection", "TritonElectronMassRatio",
            "TritonGFactor", "TritonMagneticMoment", "TritonMagneticMomentToBohrMagnetonRatio",
            "TritonMagneticMomentToNuclearMagnetonRatio", "TritonMass", "TritonMassEnergyEquivalent",
            "TritonMassEnergyEquivalentInMeV", "TritonMassInU", "TritonMolarMass", "TritonProtonMassRatio",
            "WeakMixingAngle",
        ),
        "Physico-chemical constants": (
            "AtomicMassConstant", "AtomicMassConstantEnergyEquivalent",
            "AtomicMassConstantEnergyEquivalentInMeV", "AvogadroConstant", "BoltzmannConstant",
            "BoltzmannConstantInEvperK", "BoltzmannConstantInHzperK",
            "BoltzmannConstantInInverseMetersPerKelvin", "FaradayConstant",
            "FaradayConstantForConventionalElectricCurrent", "FirstRadiationConstant",
            "FirstChandrasekharConstant", "SecondChandrasekharConstant",
            "FirstRadiationConstantForSpectralRadiance", "LoschmidtConstant27315K_100Kpa",
            "LoschmidtConstant27315K_101325Kpa", "MolarGasConstant", "MolarPlanckConstant",
            "MolarPlanckConstantTimesC", "MolarVolumeOfIdealGas273_15K_100kPa",
            "MolarVolumeOfIdealGas273_15K_101325Pa", "SackurTetrodeConstant1K_100kPa",
            "SackurTetrodeConstant1K_101325Pa", "SecondRadiationConstant", "StefanBoltzmannConstant",
            "WienFrequencyDisplacementLawConstant", "WienWavelengthDisplacementLawConstant",
        ),
        "Adopted values": (
            "ConventionalValueOfJosephsonConstant", "ConventionalValueOfVonKlitzingConstant",
            "MolarMassConstant", "MolarMassOfCarbon12", "StandardAccelerationOfGravity",
            "StandardAtmosphere", "StandardStatePressure",
        ),
        "Non-SI units": (
            "AtomicUnitOf1stHyperpolarizability", "AtomicUnitOf2ndHyperpolarizability",
            "AtomicUnitOfAction", "AtomicUnitOfCharge", "AtomicUnitOfChargeDensity", "AtomicUnitOfCurrent",
            "AtomicUnitOfElectricDipoleMoment", "AtomicUnitOfElectricField",
            "AtomicUnitOfElectricFieldGradient", "AtomicUnitOfElectricPolarizability",
            "AtomicUnitOfElectricPotential", "AtomicUnitOfElectricQuadrupoleMoment", "AtomicUnitOfEnergy",
            "AtomicUnitOfForce", "AtomicUnitOfLength", "AtomicUnitOfMagneticDipoleMoment",
            "AtomicUnitOfMagneticFluxDensity", "AtomicUnitOfMagnetizability", "AtomicUnitOfMass",
            "AtomicUnitOfMomentum", "AtomicUnitOfPermittivity", "AtomicUnitOfTime", "AtomicUnitOfVelocity",
            "NaturalUnitOfAction", "NaturalUnitOfActionInEvS", "NaturalUnitOfEnergy",
            "NaturalUnitOfEnergyInMeV", "NaturalUnitOfLength", "NaturalUnitOfMass",
            "NaturalUnitOfMomentum", "NaturalUnitOfMomentumInMeVperC", "NaturalUnitOfTime",
            "NaturalUnitOfVelocity", "ElectronVolt", "PlanckConstantOver2PiTimesCInMeVFm",
            "SpeedOfLightInVacuum", "UnifiedAtomicMassUnit",
        ),
        "X-ray values": (
            "AngstromStar", "CuXUnit", "LatticeParameterOfSilicon", "MoXUnit", "MolarVolumeOfSilicon",
            "_220LatticeSpacingOfSilicon",
        ),
        "Conversion factors for energy equivalents": (
            "AtomicMassUnitElectronVoltRelationship", "AtomicMassUnitHartreeRelationship",
            "AtomicMassUnitHertzRelationship", "AtomicMassUnitInverseMeterRelationship",
            "AtomicMassUnitJouleRelationship", "AtomicMassUnitKelvinRelationship",
            "AtomicMassUnitKilogramRelationship", "HartreeAtomicMassUnitRelationship",
            "HartreeElectronVoltRelationship", "HartreeHertzRelationship",
            "HartreeInverseMeterRelationship", "HartreeJouleRelationship", "HartreeKelvinRelationship",
            "HartreeKilogramRelationship", "HertzAtomicMassUnitRelationship",
            "HertzElectronVoltRelationship", "HertzHartreeRelationship", "HertzInverseMeterRelationship",
            "HertzJouleRelationship", "HertzKelvinRelationship", "HertzKilogramRelationship",
            "InverseMeterAtomicMassUnitRelationship", "InverseMeterElectronVoltRelationship",
            "InverseMeterHartreeRelationship", "InverseMeterHertzRelationship",
            "InverseMeterJouleRelationship", "InverseMeterKelvinRelationship",
            "InverseMeterKilogramRelationship", "JouleAtomicMassUnitRelationship",
            "JouleElectronVoltRelationship", "JouleHartreeRelationship", "JouleHertzRelationship",
            "JouleInverseMeterRelationship", "JouleKelvinRelationship", "JouleKilogramRelationship",
            "KelvinAtomicMassUnitRelationship", "KelvinElectronVoltRelationship",
            "KelvinHartreeRelationship", "KelvinHertzRelationship", "KelvinInverseMeterRelationship",
            "KelvinJouleRelationship", "KelvinKilogramRelationship", "KilogramAtomicMassUnitRelationship",
            "KilogramElectronVoltRelationship", "KilogramHartreeRelationship", "KilogramHertzRelationship",
            "KilogramInverseMeterRelationship", "KilogramJouleRelationship", "KilogramKelvinRelationship",
        ),
    },
    "MathematicalConstants": {
        "Complex mathematical constants": (
            "PowerTowerOfI", "i",
        ),
        "Basic math constants": (
            "PI", "e", "NaN", "Infinity", "EulerMascheroni", "GoldenRatio", "InverseGoldenRatio",
            "SilverRatio",
        ),
        "Classical, named math constants": (
            "AperysConstant", "ConwaysConstant", "KhinchinsConstant", "ApéryConstantζ3", "ArtinConstant",
            "BernsteinConstantβ", "BlazysConstant", "BrunConstantforTwinPrimesB4",
            "BrunConstantforPrimeCousinsB4", "BrunConstantforPrimeQuadruplesB4", "CahenConstantC",
            "CatalanConstantC", "ChampernowneConstantC10", "ContinuedFractionsConstant",
            "ConwayConstantλ3", "DelianConstant", "EmbreeTrefethenConstantβ", "ErdösBorweinConstant",
            "FeigenbaumReductionParameterα", "FeigenbaumBifurcationVelocityδ", "FransénRobinsonConstant",
            "GaussConstantG", "GaussKuzminWirsingConstantλ1", "GelfondConstant",
            "GelfondSchneiderConstant", "GerverMovingSofaConstant", "HammersleyUpperBoundOnGerverConstant",
            "GibbsConstantG", "WilbrahamGibbsConstantG", "GlaisherKinkelinConstantA",
            "GolombDickmanConstantλ", "GompertzConstantG", "GrossmannConstant", "KhinchinConstantK_K0",
            "KhinchinLévyConstantγ", "KnuthRandomgeneratorsConstant", "KolakoskiConstantγ",
            "LandauRamanujanConstant", "LaplaceLimitConstantλ", "LemniscateConstantL",
            "FirstLemniscateConstantLA", "SecondLemniscateConstantLB", "LévyConstantγ",
            "LiouvilleConstant", "LochConstant", "MadelungConstantM3", "MeisselMertensConstantB1",
            "MillsConstantA", "MKBconstant", "MRBconstant", "OmegaConstantW1", "OtterConstantα",
            "OtterAsymptoticConstantβuForUnrootedTrees", "OtterAsymptoticConstantβrForRootedTrees",
            "PlasticNumberρ", "SilverConstant", "PogsonRatio", "PolyaRandomwalkConstantp3",
            "PrévostConstant", "ReciprocalEvenFibonacciConstant", "ReciprocalOddFibonacciConstant",
            "RényiParkingConstantm", "SalemNumberσ1", "SierpinskiConstantK", "SoldnerConstantμ",
            "SomosQuadraticRecurrenceConstant", "ShallWilsonOrTwinPrimesConstantC2", "TheodorusConstant",
            "ViswanathConstant",
        ),
        "Geometry constants": (
            "MagicAngle", "ComplementaryMagicAngle", "TetrahedralAngle", "ComplementaryTetrahedralAngle",
            "GravitoidConstant", "MinimumAreaOfAConstantWidthFigure", "MoserWormConstant",
            "SquareDrillConstant", "UniversalParabolicConstant",
        ),
    },
}


def _index_groups():
    result = {}
    for kind, groups in GROUPS.items():
        for category, names in groups.items():
            for name in names:
                key = (kind, name)
                if key in result:
                    raise ValueError("Duplicate subject assignment: " + kind + "." + name)
                result[key] = category
    return result


_CATEGORIES = _index_groups()


def subject_category(kind, name):
    """Require one reviewed subject for this exact canonical name."""
    try:
        return _CATEGORIES[(kind, name)]
    except KeyError:
        raise KeyError("No reviewed subject category for " + kind + "." + name) from None
