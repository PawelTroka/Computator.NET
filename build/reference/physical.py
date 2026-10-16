"""Physical quantities: definitions and units, never a numerical-constant update."""
from common import A, NIST
# e in these formulas is physical elementary charge, not the TSL Euler constant.
METROLOGY=('The stored value is the application’s legacy binary64 value, not a live CODATA lookup or an uncertainty-bearing measurement. '
           'Use the cited current NIST/SI definition for precision work; independently stored related constants may not be numerically consistent.')
BASE={
'c':('Speed of light in vacuum','Propagation speed of light in vacuum and conversion between time and length.','c = 299792458 m/s exactly in the SI.','m s⁻¹','The short name and SpeedOfLightInVacuum represent the same physical quantity.'),
'g':('Legacy gravitational acceleration','A stored near-Earth acceleration used in elementary mechanics.','F=m·g; free-fall distance from rest is ½g·t².','m s⁻²','The stored 9.80625 is NOT the conventional standard acceleration 9.80665. Use StandardAccelerationOfGravity for the conventional value; local gravity varies with location.'),
'h':('Planck constant — short name','Relates photon energy to ordinary frequency, E=h·ν.','h = 6.62607015×10⁻³⁴ J·s exactly in the modern SI.','J s','This short name and PlanckConstant have different historical stored approximations. Neither should be treated as the current exact SI value.'),
'G':('Newtonian gravitational constant — short name','Sets the strength of Newtonian gravitational attraction.','F=G·m₁·m₂/r².','m³ kg⁻¹ s⁻²','The short name G and NewtonianConstantOfGravitation have slightly different stored values. G is experimentally determined, not an SI defining constant.'),
'MagneticConstant':('Vacuum magnetic permeability','Relates magnetic flux density and magnetic field strength in vacuum.','B=μ₀·H.','N A⁻²','The code stores the pre-2019 exact convention 4·PI×10⁻⁷. In the modern SI μ₀ is experimentally determined through the fine-structure constant, not exactly that number.'),
'ElectricConstant':('Vacuum electric permittivity','Relates electric displacement to electric field in vacuum and enters Coulomb’s law.','D=ε₀·E; ε₀=1/(μ₀c²).','F m⁻¹','Not exact in the modern SI. Do not assume the rounded independently stored μ₀ and ε₀ obey their relation to every displayed digit.'),
'ImpedanceOfVacuum':('Characteristic impedance of vacuum','Ratio of electric-field to magnetic-field strength in a plane electromagnetic wave in vacuum.','Z₀=μ₀c=sqrt(μ₀/ε₀).','Ω',''),
'NewtonianConstantOfGravitation':('Newtonian gravitational constant','Sets the proportionality in the inverse-square gravitational force law.','F=G·m₁·m₂/r².','m³ kg⁻¹ s⁻²','This measured constant is not exact; distinguish it from a local acceleration g.'),
'NewtonianConstantOfGravitationOverHBarC':('Gravitational coupling in particle-energy units','Newton’s gravitational constant divided by ℏc, represented using GeV-based high-energy units.','G/(ℏc).','(GeV/c²)⁻²','This is not dimensionless. Multiplying by a mass squared in GeV/c² gives a dimensionless gravitational coupling.'),
'PlanckConstant':('Planck constant','Converts ordinary frequency to quantum energy.','E=h·ν.','J s','Modern SI fixes h exactly at 6.62607015×10⁻³⁴ J·s; the stored legacy approximation differs.'),
'PlanckConstantInEvS':('Planck constant in electronvolt seconds','Converts photon frequency in hertz to energy in electronvolts.','E[eV]=h[eV·s]·ν[Hz].','eV s',''),
'PlanckConstantOver2Pi':('Reduced Planck constant','Quantum of angular momentum; uses angular frequency rather than cycles per second.','ℏ=h/(2π); E=ℏω.','J s',''),
'PlanckConstantOver2PiInEvS':('Reduced Planck constant in electronvolt seconds','Converts angular frequency to energy in electronvolts.','E[eV]=ℏ[eV·s]·ω[s⁻¹].','eV s',''),
'PlanckConstantOver2PiTimesCInMeVFm':('Reduced Planck constant times c','Energy–length conversion useful in nuclear and particle physics.','ℏc.','MeV fm','A femtometre is 10⁻¹⁵ m.'),
'PlanckLength':('Planck length','Natural length scale formed from quantum mechanics, gravitation and light speed.','lP=sqrt(ℏG/c³).','m','A natural scale, not a measured claim of a minimum possible length. The stored 1.61619997×10⁻³⁵ m differs from the 2010 CODATA central value 1.616199×10⁻³⁵ m; its extra digits are not additional precision.'),
'PlanckTime':('Planck time','Light travel time across the Planck length.','tP=lP/c=sqrt(ℏG/c⁵).','s','The stored 5.3910632×10⁻⁴⁴ s differs from the 2010 CODATA central value 5.39106×10⁻⁴⁴ s; its extra digits are not additional precision.'),
'PlanckMass':('Planck mass','Mass scale at which the dimensionless gravitational coupling becomes order one.','mP=sqrt(ℏc/G).','kg','Uses the unreduced Planck-mass convention; the reduced gravitational mass scale differs by sqrt(8π). The stored 2.1765113×10⁻⁸ kg differs from the 2010 CODATA central value 2.17651×10⁻⁸ kg; its extra digits are not additional precision.'),
'PlanckMassEnergyEquivalentInGev':('Planck mass energy','Energy equivalent of the unreduced Planck mass.','EP=mP·c².','GeV','This is an energy, not the mass value in kilograms.'),
'PlanckTemperature':('Planck temperature','Temperature associated with the Planck mass energy.','TP=mP·c²/kB.','K',''),
'ElementaryCharge':('Elementary charge','Positive magnitude of the charge of a proton; an electron has charge −e.','e = 1.602176634×10⁻¹⁹ C exactly in the modern SI.','C','The identifier ElementaryCharge is distinct from the mathematical constant e in TSL.'),
'ElementaryChargeOverH':('Elementary charge divided by Planck constant','Frequency-per-voltage conversion underlying quantum electrical relations.','e/h.','A J⁻¹','Equivalently Hz/V. Physical e is ElementaryCharge, not Euler’s number.'),
'BohrMagneton':('Bohr magneton','Natural magnetic-moment scale for an electron.','μB=eℏ/(2me).','J T⁻¹','A magnetic moment, not a magnetic field.'),
'NuclearMagneton':('Nuclear magneton','Magnetic-moment scale defined using the proton mass.','μN=eℏ/(2mp).','J T⁻¹','Nuclear magnetic moments need not be an integer number of nuclear magnetons.'),
'BohrRadius':('Bohr radius','Atomic length scale equal to the ground-state orbit radius in the simplest hydrogen model.','a₀=ℏ/(me·c·α).','m',''),
'ClassicalElectronRadius':('Classical electron radius','Length scale obtained by equating an electrostatic energy scale to the electron rest energy.','re=e²/(4π ε₀ me c²)=α²a₀.','m','Not a measured geometric radius of an extended electron.'),
'ComptonWavelength':('Electron Compton wavelength','Photon wavelength whose quantum energy equals the electron rest energy.','λC=h/(me·c).','m','This is the ordinary Compton wavelength; the reduced one is smaller by 2π.'),
'ComptonWavelengthOver2Pi':('Reduced electron Compton wavelength','Electron quantum length scale using ℏ rather than h.','λC/(2π)=ℏ/(me·c).','m',''),
'ConductanceQuantum':('Conductance quantum','Conductance scale of a fully transmitting spin-degenerate electronic channel.','G₀=2e²/h.','S','The factor two represents spin degeneracy.'),
'InverseOfConductanceQuantum':('Resistance quantum for a spin-degenerate channel','Reciprocal of the conductance quantum.','1/G₀=h/(2e²).','Ω','Half of the von Klitzing resistance h/e².'),
'MagneticFluxQuantum':('Superconducting flux quantum','Magnetic flux associated with the charge 2e of a Cooper pair.','Φ₀=h/(2e).','Wb',''),
'JosephsonConstant':('Josephson constant','Converts Josephson-junction voltage to oscillation frequency.','KJ=2e/h; f=KJ·V.','Hz V⁻¹',''),
'VonKlitzingConstant':('Von Klitzing constant','Resistance scale of the integer quantum Hall effect.','RK=h/e²; Hall plateau resistance RK/i.','Ω','The integer plateau index i here is not the TSL imaginary-unit constant.'),
'ConventionalValueOfJosephsonConstant':('1990 conventional Josephson value','The historical KJ-90 assigned value used to realize conventional electrical voltage.','KJ-90 = 483597.9×10⁹ Hz/V.','Hz V⁻¹','A historical conventional assignment, not the present SI Josephson constant. The 1990 electrical conventions were replaced by the revised SI in 2019.'),
'ConventionalValueOfVonKlitzingConstant':('1990 conventional von Klitzing value','Historical RK-90 assigned value for conventional electrical resistance.','RK-90 = 25812.807 Ω.','Ω','Do not interchange a conventional 1990 value and its modern SI value without conversion.'),
'FineStructureConstant':('Fine-structure constant','Dimensionless low-energy electromagnetic coupling strength.','α=e²/(4π ε₀ℏc).','1','Experimentally determined; do not confuse physical α with the GSL order/shape parameter of the same letter.'),
'InverseFineStructureConstant':('Inverse fine-structure constant','Reciprocal of the electromagnetic coupling α.','α⁻¹.','1','Stored independently from FineStructureConstant, so rounding need not preserve an exact reciprocal.'),
'FermiCouplingConstant':('Fermi coupling constant','Low-energy effective strength of weak-interaction four-fermion coupling, in the usual particle-physics convention.','GF/(ℏc)³.','GeV⁻²','The supplied number uses natural-unit conventions; do not treat it as an SI joule-based coefficient.'),
'WeakMixingAngle':('Squared weak mixing sine','Electroweak mixing parameter in the on-shell mass-ratio convention.','sin²θW=1−(mW/mZ)².','1','Despite the identifier, this stores sin² of an angle, NOT θW in radians or degrees. The value is convention- and determination-dependent.'),
'RydbergConstant':('Rydberg constant for infinite nuclear mass','Wavenumber scale in hydrogenic spectral formulas.','R∞=α²me c/(2h); 1/λ=R∞(1/n₁²−1/n₂²).','m⁻¹','Finite nuclear mass and other corrections are separate.'),
'RydbergConstantTimesCInHz':('Rydberg frequency','Frequency equivalent of the infinite-mass Rydberg energy.','R∞c.','Hz',''),
'RydbergConstantTimesHcInJ':('Rydberg energy in joules','Infinite-mass hydrogenic binding-energy scale.','R∞hc=Eh/2.','J',''),
'RydbergConstantTimesHcInEv':('Rydberg energy in electronvolts','Infinite-mass hydrogenic binding-energy scale expressed in eV.','R∞hc=Eh/2.','eV',''),
'HartreeEnergy':('Hartree energy','Atomic-unit energy scale, twice the Rydberg energy.','Eh=α²me c²=ℏ²/(me a₀²).','J',''),
'HartreeEnergyInEv':('Hartree energy in electronvolts','Atomic-unit energy scale expressed in electronvolts.','Eh/e when e is the joules-per-electronvolt conversion.','eV',''),
'ThomsonCrossSection':('Thomson scattering cross section','Low-energy total cross section for photon scattering by a free electron.','σT=(8π/3)re².','m²','Valid in the classical low-photon-energy limit, not the full relativistic Compton regime.'),
'QuantumOfCirculation':('Quantum circulation h/(2me)','Circulation scale formed with a pair of electron masses.','h/(2me).','m² s⁻¹','Use the mass appropriate to the physical system; this constant is specifically electron-mass based.'),
'QuantumOfCirculationTimes2':('Quantum circulation h/me','Twice the electron-pair circulation scale.','h/me.','m² s⁻¹',''),
'ProtonMagneticShieldingCorrection':('Proton magnetic shielding correction','Fractional shielding correction relating a free proton moment to the reference shielded proton in water.','μp′=μp(1−σp).','1','The reference is a spherical water sample at 25 °C; this is not a universal correction for every proton-containing sample.'),
'AtomicMassConstant':('Atomic mass constant','One twelfth of the mass of a free neutral carbon-12 atom at rest in its ground state.','mu=m(¹²C)/12.','kg','The dalton/unified atomic mass unit uses this mass; its value in kilograms is experimentally determined.'),
'AtomicMassConstantEnergyEquivalent':('Atomic mass unit energy','Rest-energy equivalent of one unified atomic mass unit.','mu c².','J',''),
'AtomicMassConstantEnergyEquivalentInMeV':('Atomic mass unit energy in MeV','Rest-energy equivalent of one unified atomic mass unit in nuclear-energy units.','mu c².','MeV',''),
'AvogadroConstant':('Avogadro constant','Number of specified elementary entities per mole.','NA=6.02214076×10²³ mol⁻¹ exactly in the modern SI.','mol⁻¹','A mole must refer to a specified entity such as atoms, molecules or electrons; the stored pre-2019 number is not the exact modern definition.'),
'BoltzmannConstant':('Boltzmann constant','Converts thermodynamic temperature to an energy scale per particle.','Escale=kB T; kB=1.380649×10⁻²³ J/K exactly in the modern SI.','J K⁻¹','The stored legacy approximation is retained, not updated to the modern exact value.'),
'BoltzmannConstantInEvperK':('Boltzmann constant in eV/K','Thermal energy per kelvin expressed in electronvolts.','kB T in eV for T in K.','eV K⁻¹',''),
'BoltzmannConstantInHzperK':('Boltzmann frequency per kelvin','Ordinary-frequency equivalent of thermal energy.','kB/h.','Hz K⁻¹',''),
'BoltzmannConstantInInverseMetersPerKelvin':('Boltzmann wavenumber per kelvin','Reciprocal-wavelength equivalent of thermal energy.','kB/(hc).','m⁻¹ K⁻¹','This is spectroscopic wavenumber, not angular wavenumber.'),
'FaradayConstant':('Faraday constant','Electric charge per mole of elementary charges.','F=NA·e.','C mol⁻¹',''),
'FaradayConstantForConventionalElectricCurrent':('Faraday constant in conventional electrical units','Charge per mole expressed using the historical 1990 electrical-unit realization.','F90.','C₉₀ mol⁻¹','A historical electrical-unit quantity. Do not substitute it for the modern SI FaradayConstant without conversion.'),
'FirstRadiationConstant':('First radiation constant','Coefficient in Planck’s wavelength law for hemispherical blackbody spectral exitance.','c₁=2πhc²; Mλ=c₁/[λ⁵(exp(c₂/(λT))−1)].','W m²','Includes a factor π relative to the spectral-radiance constant.'),
'FirstRadiationConstantForSpectralRadiance':('First radiation constant for radiance','Coefficient in Planck’s wavelength law per steradian.','c₁L=2hc²; Bλ=c₁L/[λ⁵(exp(c₂/(λT))−1)].','W m² sr⁻¹','Do not confuse radiance per solid angle with hemispherically integrated exitance.'),
'SecondRadiationConstant':('Second radiation constant','Temperature–wavelength scale in the Planck exponential.','c₂=hc/kB.','m K',''),
'StefanBoltzmannConstant':('Stefan–Boltzmann constant','Relates total blackbody radiant exitance to the fourth power of absolute temperature.','M=σT⁴; σ=2π⁵kB⁴/(15h³c²).','W m⁻² K⁻⁴','An emissivity factor is needed for a nonideal gray body.'),
'MolarGasConstant':('Molar gas constant','Energy-per-mole temperature scale in the ideal-gas law.','R=NA kB; pV=nRT.','J mol⁻¹ K⁻¹',''),
'MolarMassConstant':('Molar mass constant','Converts relative atomic mass to molar mass.','Mu=NA·mu; M=Ar·Mu.','kg mol⁻¹','The code stores the historical exact 0.001 kg/mol. In the post-2019 SI the molar mass constant is no longer exact.'),
'MolarMassOfCarbon12':('Carbon-12 molar mass','Mass of one mole of free carbon-12 atoms in their ground state.','M(¹²C)=12·Mu.','kg mol⁻¹','The historical exact 0.012 kg/mol is retained; its modern SI realization is no longer exactly fixed by definition.'),
'MolarPlanckConstant':('Molar Planck constant','Converts frequency to energy per mole of quanta.','NA·h.','J s mol⁻¹',''),
'MolarPlanckConstantTimesC':('Molar Planck constant times c','Converts reciprocal wavelength to energy per mole of photons.','NA·h·c.','J m mol⁻¹',''),
'StandardAccelerationOfGravity':('Conventional standard gravity','Conventional acceleration used to define standard weight-based units.','gn=9.80665 m/s² exactly.','m s⁻²','This is a defined reference, not the gravity at every location. The short constant g in this application stores a different number.'),
'StandardAtmosphere':('Standard atmosphere','Conventional pressure unit.','1 atm=101325 Pa exactly.','Pa','Different from the 100000 Pa standard-state reference pressure.'),
'StandardStatePressure':('Standard-state pressure','Conventional pressure reference of one bar.','p°=100000 Pa exactly.','Pa','Different from one standard atmosphere, 101325 Pa.'),
'WienFrequencyDisplacementLawConstant':('Wien frequency displacement constant','Frequency of the maximum of Planck spectral radiance per unit frequency, divided by temperature.','νmax/T=(kB/h)x, where 3(1−exp(−x))=x and x>0.','Hz K⁻¹','The frequency peak is not c divided by the wavelength peak; changing spectral density changes the peak location.'),
'WienWavelengthDisplacementLawConstant':('Wien wavelength displacement constant','Wavelength of the maximum of Planck spectral radiance per unit wavelength, times temperature.','λmax·T=(hc/kB)/x, where 5(1−exp(−x))=x and x>0.','m K','The maximum depends on whether the spectrum is defined per wavelength or per frequency.'),
'AngstromStar':('Ångström star','Historical X-ray wavelength unit, slightly different from the exact ångström.','1 Å*=the stored length in metres.','m','The ångström without a star is exactly 10⁻¹⁰ m; Å* is a separate calibrated X-ray unit.'),
'CuXUnit':('Copper X unit','Historical X-ray length unit tied to the Cu Kα₁ line.','λ(Cu Kα₁)=1537.400 xu(Cu).','m','Not the same calibrated unit as the molybdenum X unit.'),
'MoXUnit':('Molybdenum X unit','Historical X-ray length unit tied to the Mo Kα₁ line.','λ(Mo Kα₁)=707.831 xu(Mo).','m','','https://physics.nist.gov/cuu/pdf/JPCRD2010CODATA.pdf#page=41'),
'LatticeParameterOfSilicon':('Silicon lattice parameter','Cubic unit-cell edge length for the reference silicon crystal.','a(Si).','m','Crystal conditions and isotope composition matter; use the source reference conditions for precision diffraction.'),
'_220LatticeSpacingOfSilicon':('Silicon {220} lattice spacing','Distance between adjacent {220} planes of the reference silicon lattice.','d₂₂₀=a/sqrt(8).','m','The leading underscore is part of the application identifier.'),
'MolarVolumeOfSilicon':('Silicon molar volume','Reference volume occupied by one mole of crystalline silicon atoms.','Vm=NA·a³/8 for a diamond-cubic cell.','m³ mol⁻¹','Applies to the specified reference crystal conditions, not every temperature or composition.'),
'ElectronVolt':('Electronvolt in joules','Energy gained by one elementary positive charge through a potential difference of one volt.','1 eV=1.602176634×10⁻¹⁹ J exactly in the modern SI.','J','The stored conversion is historical. Physical charge e is not the TSL Euler-number constant.'),
}
ALIASES={'SpeedOfLightInVacuum':'c','CharacteristicImpedanceOfVacuum':'ImpedanceOfVacuum','UnifiedAtomicMassUnit':'AtomicMassConstant'}
ATOMIC={
 '1stHyperpolarizability':('first hyperpolarizability','e³a₀³/Eh²','C³ m³ J⁻²'),
 '2ndHyperpolarizability':('second hyperpolarizability','e⁴a₀⁴/Eh³','C⁴ m⁴ J⁻³'),
 'Action':('action','ℏ','J s'),'Charge':('charge','e','C'),'ChargeDensity':('charge density','e/a₀³','C m⁻³'),
 'Current':('electric current','eEh/ℏ','A'),'ElectricDipoleMoment':('electric dipole moment','ea₀','C m'),
 'ElectricField':('electric field','Eh/(ea₀)','V m⁻¹'),'ElectricFieldGradient':('electric field gradient','Eh/(ea₀²)','V m⁻²'),
 'ElectricPolarizability':('electric polarizability','e²a₀²/Eh','C² m² J⁻¹'),
 'ElectricPotential':('electric potential','Eh/e','V'),'ElectricQuadrupoleMoment':('electric quadrupole moment','ea₀²','C m²'),
 'Energy':('energy','Eh','J'),'Force':('force','Eh/a₀','N'),'Length':('length','a₀','m'),
 'MagneticDipoleMoment':('magnetic dipole moment','eℏ/me = 2μB','J T⁻¹'),
 'MagneticFluxDensity':('magnetic flux density','ℏ/(ea₀²)','T'),
 'Magnetizability':('magnetizability','e²a₀²/me','J T⁻²'),'Mass':('mass','me','kg'),
 'Momentum':('momentum','ℏ/a₀','kg m s⁻¹'),'Permittivity':('permittivity','e²/(a₀Eh) = 4π ε₀','F m⁻¹'),
 'Time':('time','ℏ/Eh','s'),'Velocity':('velocity','αc','m s⁻¹')}
NATURAL={
 'Action':('action','ℏ','J s'),'ActionInEvS':('action in electronvolt seconds','ℏ','eV s'),
 'Energy':('energy','me c²','J'),'EnergyInMeV':('energy in MeV','me c²','MeV'),
 'Length':('length','ℏ/(me c)','m'),'Mass':('mass','me','kg'),
 'Momentum':('momentum','me c','kg m s⁻¹'),'MomentumInMeVperC':('momentum in MeV/c','me c','MeV/c'),
 'Time':('time','ℏ/(me c²)','s'),'Velocity':('velocity','c','m s⁻¹')}
PARTICLES={'AlphaParticle':('alpha particle','helium-4 nucleus, not the neutral atom','mα'),
 'Electron':('electron','negatively charged lepton','me'),'Muon':('muon','charged second-generation lepton','mμ'),
 'Tau':('tau','charged third-generation lepton','mτ'),'Proton':('proton','hydrogen-1 nucleus','mp'),
 'Neutron':('neutron','electrically neutral nucleon','mn'),'Deuteron':('deuteron','hydrogen-2 nucleus','md'),
 'Triton':('triton','hydrogen-3 nucleus','mt'),'Helion':('helion','helium-3 nucleus, not the neutral atom','mh'),
 'ShieldedProton':('shielded proton','proton in the reference water environment','mp'),
 'ShieldedHelion':('shielded helion','helium-3 nucleus shielded by the electrons in the reference helium environment','mh')}
UNIT_ENERGY={'AtomicMassUnit':('u','mu c²'),'ElectronVolt':('eV','e·1 V'),'Hartree':('Eh','Eh'),
 'Hertz':('Hz','h·1 Hz'),'InverseMeter':('m⁻¹','hc·1 m⁻¹'),'Joule':('J','1 J'),
 'Kelvin':('K','kB·1 K'),'Kilogram':('kg','1 kg·c²')}

def finish(item, values, title, definition, formula, unit, note='', url=NIST):
    value=values[item['signature']]
    if 'ShieldedProton' in item['signature']:
        note+=' Shielded proton values refer to a spherical water sample at 25 °C.'
    if 'ShieldedHelion' in item['signature']:
        note+=' Shielded helion values refer to a spherical helium gas sample at 25 °C.'
    return A(title,definition,formula,'Unit: '+unit+'. Stored application value: '+value+'.',
             (note+' ' if note else '')+METROLOGY,url=url,category='Physical constants')

def describe(item, values):
    original=item['signature']; name=ALIASES.get(original,original)
    if name in BASE:return finish(item,values,*BASE[name])
    if name.startswith('AtomicUnitOf'):
        suffix=name[len('AtomicUnitOf'):]; label,formula,unit=ATOMIC[suffix]
        return finish(item,values,'Atomic unit of '+label,
          'Conversion from one Hartree atomic unit of '+label+' to the stated SI unit. Atomic units use the electron mass me, elementary charge e, reduced Planck constant ℏ and Bohr radius a₀.',formula,unit,
          'Multiply an atomic-unit value by this factor to obtain the stated unit. Do not mix Hartree and Rydberg atomic-unit conventions.')
    if name.startswith('NaturalUnitOf'):
        label,formula,unit=NATURAL[name[len('NaturalUnitOf'):]]
        return finish(item,values,'Electron-based natural unit of '+label,
          'Conversion for the electron-based natural-unit system using me, c and ℏ.',formula,unit,
          'This is the electron-mass convention, not a Planck-unit definition. Multiply a number expressed in this natural unit by the stored factor.')
    if name.endswith('Relationship'):
        middle=name[:-len('Relationship')]
        pairs=[(a,b) for a in UNIT_ENERGY for b in UNIT_ENERGY if a!=b and a+b==middle]
        if len(pairs)!=1:raise KeyError(name)
        a,b=pairs[0];ua,ea=UNIT_ENERGY[a];ub,eb=UNIT_ENERGY[b]
        return finish(item,values,ua+' to '+ub+' energy-equivalent conversion',
          'Converts a numeric quantity in '+ua+' to its energy-equivalent quantity in '+ub+'. Mass uses E=mc², frequency E=hν, reciprocal wavelength E=hc/λ and temperature E=kB T.',
          'Output['+ub+'] = input['+ua+'] × '+original+'.',ub+' per '+ua,
          'This relates equivalent energies; it does not assert that a thermal system has a single photon frequency. Inverse metres mean ordinary spectroscopic wavenumber, not radians per metre.')
    if name.startswith('LoschmidtConstant') or name.startswith('MolarVolumeOfIdealGas'):
        atmosphere='101325' in name; pressure='101325 Pa (1 atm)' if atmosphere else '100000 Pa (1 bar)'
        isnumber=name.startswith('Loschmidt')
        return finish(item,values,('Ideal-gas number density' if isnumber else 'Ideal-gas molar volume')+' at 273.15 K',
          ('Number of gas particles per unit volume' if isnumber else 'Volume per mole of an ideal gas')+' at T=273.15 K and p='+pressure+'.',
          'n₀=p/(kB T).' if isnumber else 'Vm=RT/p.', 'm⁻³' if isnumber else 'm³ mol⁻¹',
          'The pressure in the identifier is historical shorthand; this entry uses '+pressure+', not 101325 kPa.')
    if name.startswith('SackurTetrodeConstant'):
        p='101325 Pa' if '101325' in name else '100000 Pa'
        return finish(item,values,'Sackur–Tetrode entropy constant at '+p,
          'Dimensionless entropy reference for a monatomic ideal gas, using T₀=1 K, reference mass mu and the stated pressure p₀.',
          'S₀/R = 5/2 + ln[(2πmu kB T₀)^(3/2)·kB T₀/(h³p₀)].','1',
          'Used in S/(nR)=S₀/R+(3/2)ln(m/mu)+(5/2)ln(T/T₀)−ln(p/p₀). Not an entropy in joules by itself.')
    if name in ('FirstChandrasekharConstant','SecondChandrasekharConstant'):
        first=name.startswith('First');c='Γ(4/3)/(4π/3)^(1/3)' if first else '(1/(2π))^(1/3)'
        return finish(item,values,('Mean' if first else 'Most probable')+' nearest-neighbour coefficient',
          'For a homogeneous three-dimensional Poisson point process of number density n, the '+('mean' if first else 'mode of the')+' nearest-neighbour distance scales as this coefficient times n^(−1/3).',
          'r'+('mean' if first else 'mode')+' = '+c+'·n^(−1/3).','1',
          'This is Chandrasekhar’s spatial-statistics coefficient, not the white-dwarf Chandrasekhar mass limit.',
          'https://oeis.org/'+('A213054' if first else 'A213055'))
    for root,symbol in [('BohrMagneton','μB'),('NuclearMagneton','μN')]:
        if name.startswith(root+'In'):
            suffix=name[len(root+'In'):]
            units={'EvperT':('eV T⁻¹',symbol+'/e'), 'HzperT':('Hz T⁻¹',symbol+'/h'),
                   'MhzperT':('MHz T⁻¹',symbol+'/(h·10⁶)'),
                   'InverseMetersPerTesla':('m⁻¹ T⁻¹',symbol+'/(hc)'), 'KperT':('K T⁻¹',symbol+'/kB')}
            unit,formula=units[suffix]
            return finish(item,values,root.replace('Magneton',' magneton')+' in '+unit,
              'Magneton expressed as an energy, frequency, wavenumber or temperature shift per tesla in the stated convention.',formula,unit,
              'Frequency uses h, not ℏ. The MHz representation has an extra factor of one million relative to Hz.')
    if name.startswith('NeutronProtonMassDifference'):
        suffix=name[len('NeutronProtonMassDifference'):]
        units={'':('kg','mn−mp'),'InU':('u','(mn−mp)/mu'),'EnergyEquivalent':('J','(mn−mp)c²'),
               'EnergyEquivalentInMeV':('MeV','(mn−mp)c²')}
        unit,formula=units[suffix]
        return finish(item,values,'Neutron–proton mass difference','Positive difference between the free neutron and proton rest masses, in the stated mass or energy unit.',formula,unit)
    # Closed, explicit particle/property grammar. Unknown quantities fail authoring.
    for particle in sorted(PARTICLES,key=len,reverse=True):
        if not name.startswith(particle):continue
        rest=name[len(particle):];label,identity,mass=PARTICLES[particle]
        if rest.startswith('To'):rest=rest[2:]
        for partner in sorted(PARTICLES,key=len,reverse=True):
            if rest==partner+'MassRatio':
                label2,_,mass2=PARTICLES[partner]
                return finish(item,values,label.capitalize()+' / '+label2+' mass ratio',
                  'Dimensionless ratio of the free '+label+' rest mass to the '+label2+' rest mass, in that order.',mass+'/'+mass2,'1')
            if rest==partner+'MagneticMomentRatio':
                label2=PARTICLES[partner][0]
                return finish(item,values,label.capitalize()+' / '+label2+' magnetic-moment ratio',
                  'Signed ratio of the '+label+' magnetic moment to that of the '+label2+'.','μ('+label+')/μ('+label2+').','1',
                  'Negative sign means opposite moment orientations under the source convention. Shielded quantities refer to their specified chemical environment.')
        mass_props={'Mass':('rest mass','kg',mass),'MassInU':('relative mass in unified atomic mass units','u',mass+'/mu'),
          'MassEnergyEquivalent':('rest energy','J',mass+'c²'), 'MassEnergyEquivalentInMeV':('rest energy in MeV','MeV',mass+'c²'),
          'MolarMass':('molar mass','kg mol⁻¹','NA·'+mass),
          'ComptonWavelength':('Compton wavelength','m','h/('+mass+'c)'),
          'ComptonWavelengthOver2Pi':('reduced Compton wavelength','m','ℏ/('+mass+'c)')}
        if rest in mass_props:
            prop,unit,formula=mass_props[rest]
            return finish(item,values,label.capitalize()+' '+prop,'The '+prop+' of the '+label+' ('+identity+').',formula,unit,
              'Nuclear masses exclude atomic electrons and their binding energy.' if particle in ('AlphaParticle','Deuteron','Triton','Helion','Proton') else '')
        if rest=='ChargeToMassQuotient':
            return finish(item,values,label.capitalize()+' charge-to-mass ratio','Signed electric charge divided by rest mass. Controls electromagnetic acceleration.',('−e/' if particle=='Electron' else 'e/')+mass,'C kg⁻¹','The electron ratio is negative; proton charge is positive.')
        if rest in ('MagneticMoment','MagneticMomentToBohrMagnetonRatio','MagneticMomentToNuclearMagnetonRatio'):
            ratio=rest!='MagneticMoment'; denominator='μB' if 'Bohr' in rest else 'μN'
            return finish(item,values,label.capitalize()+' magnetic moment'+(' / '+denominator if ratio else ''),
              'Signed magnetic dipole moment of the '+label+' ('+identity+'), '+('in units of the '+('Bohr' if 'Bohr' in rest else 'nuclear')+' magneton.' if ratio else 'setting its first-order energy shift in a magnetic field.'),
              'μ('+label+')'+('/'+denominator if ratio else '; ΔE=−μ·B.'),'1' if ratio else 'J T⁻¹',
              'Shielded values include the reference electronic/chemical environment; they are not the free nuclear moment.' if particle.startswith('Shielded') else 'The sign is part of the physical convention and must not be discarded.')
        if rest=='GFactor':
            spin='1' if particle=='Deuteron' else '1/2'
            if particle in ('Electron','Muon','Tau'):
                magneton='eℏ/(2'+mass+')'
                convention='The magneton uses the '+label+' mass '+mass+'. Charged-lepton entries include the signed magnetic-moment convention.'
            else:
                magneton='μN; μN=eℏ/(2mp)'
                convention='Nuclei use the nuclear magneton, defined with the proton mass mp.'
            return finish(item,values,label.capitalize()+' g-factor',
              'Dimensionless proportionality between spin and the magnetic moment of the '+label+'.',
              'μ = g·μref·I; μref='+magneton+'; I='+spin+'.','1',
              convention+' I is the spin quantum number. A g-factor is not simply the moment in magneton units unless I=1.')
        if rest=='MagneticMomentAnomaly':
            return finish(item,values,label.capitalize()+' magnetic-moment anomaly',
              'Quantum correction to the magnitude of the charged-lepton Dirac g-factor.','a=(|g|−2)/2.','1',
              'Do not compute this as (signed g−2)/2 when the tabulated electron or muon g is negative.')
        if rest.startswith('GyromagneticRatio'):
            over=rest.endswith('Over2Pi')
            return finish(item,values,label.capitalize()+' gyromagnetic ratio'+(' / 2π' if over else ''),
              'Magnitude of the spin-precession frequency per unit magnetic field for the '+label+'.',
              '|γ|/(2π)=|μ|/(hI); value[MHz/T]=|μ[J/T]|/(h[J·s]·I·10⁶).' if over else '|γ|=|μ|/(ℏI).', 'MHz T⁻¹' if over else 's⁻¹ T⁻¹',
              'I=1/2 is the spin quantum number. '+('Over2Pi entries are in MHz/T, NOT Hz/T. The stored positive value is a magnitude; the magnetic-moment sign is stored separately.' if over else 'This is angular frequency per field, conventionally rad/s/T; the tabulated value is a magnitude.'))
        if rest=='RmsChargeRadius':
            return finish(item,values,label.capitalize()+' rms charge radius','Root-mean-square radius of the '+label+' charge distribution.','rcharge=sqrt(⟨r²⟩charge).','m','A structure parameter, not a sharp hard-sphere boundary.')
    raise KeyError('No physical definition for '+name)
