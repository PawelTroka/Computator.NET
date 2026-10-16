"""Explicit physical.BASE equations not already taught by CONSTANT_LESSONS.

Keys are canonical physical.BASE names after physical.ALIASES normalization.
Each value is (reviewed TeX, specific explanatory paragraph). These are authored
relationships, never a parser that guesses mathematics from arbitrary prose.
The generator retains the original units, stored decimal and metrology notes.

Primary reference for relationships:
https://physics.nist.gov/cuu/pdf/CODATA_RMP2016.pdf
(Tables XXXII-XXXIII).
Historical X-ray unit definitions:
https://physics.nist.gov/cuu/pdf/JPCRD2010CODATA.pdf
(section 9.4, equations 255-257). In particular, equation 256 assigns 707.831
to the Mo line. The 2014 report's section IX.A prose has a conflicting 707.400;
its Table XXXVI, and the 2010 definition and table, all specify 707.831.
The 1990 electrical assignments are recorded in the original CIPM decisions:
https://www.bipm.org/en/committees/ci/cipm/77-1988/resolution-1
https://www.bipm.org/en/committees/ci/cipm/77-1988/resolution-2
These sources establish definitions; their tabulated decimals are not substituted
for the application's historical values.
"""


BASE_EQUATIONS = {
    'MagneticConstant': (
        r'\mathbf B=\mu_0\mathbf H,\qquad\mu_0=\frac{2\alpha h}{e^2c}',
        'B is magnetic flux density and H is magnetic field strength. In vacuum their proportionality is μ₀; a material generally needs its own constitutive relation. Expressing μ₀ through α explains why its modern SI value inherits experimental uncertainty.'),
    'ElectricConstant': (
        r'\mathbf D=\varepsilon_0\mathbf E,\qquad\varepsilon_0=\frac1{\mu_0c^2}',
        'D is electric displacement and E is electric field. This vacuum relation does not replace a dielectric material’s response. The identity relates the physical quantities; rounding independent stored values can leave a small numerical residual.'),
    'ImpedanceOfVacuum': (
        r'Z_0=\frac{|\mathbf E|}{|\mathbf H|}=\mu_0c=\sqrt{\frac{\mu_0}{\varepsilon_0}}',
        'The field ratio applies to a traveling plane wave in vacuum. It uses H, measured in amperes per meter; the ratio E/B instead has the dimensions and value of a speed.'),
    'NewtonianConstantOfGravitationOverHBarC': (
        r'g_G=\frac{G}{\hbar c},\qquad\alpha_G(m)=g_Gm^2',
        'The displayed gG denotes the stored gravitational coefficient. It has inverse-mass-squared dimensions, so the coupling becomes dimensionless only after multiplying by a squared mass in the matching GeV/c² unit.'),
    'PlanckConstantInEvS': (
        r'E[\mathrm{eV}]=h[\mathrm{eV\,s}]\,\nu[\mathrm{Hz}]',
        'Square brackets here indicate numerical values in the named units. Ordinary frequency in hertz times this eV·s representation gives energy directly in electronvolts; no second joule-to-eV conversion is required.'),
    'PlanckConstantOver2PiInEvS': (
        r'E[\mathrm{eV}]=\hbar[\mathrm{eV\,s}]\,\omega[\mathrm{s^{-1}}],\qquad\omega=2\pi\nu',
        'Angular frequency is measured in radians per second. Using the same numerical frequency in hertz with this reduced constant would omit the factor 2π; the ordinary-frequency version uses h.'),
    'PlanckConstantOver2PiTimesCInMeVFm': (
        r'pc=(\hbar c)k,\qquad k=\frac{2\pi}{\lambda}',
        'For a de Broglie wavelength λ, k is angular wavenumber. Multiplying ℏc in MeV·fm by k in inverse femtometers gives pc in MeV. Using 1/λ in place of k would miss a factor 2π.'),
    'PlanckLength': (
        r'\ell_{\mathrm P}=\sqrt{\frac{\hbar G}{c^3}}',
        'The combination under the square root has dimensions of length squared. This scale combines gravitational and quantum constants; dimensional analysis by itself does not prove that shorter distances cannot exist.'),
    'PlanckTime': (
        r't_{\mathrm P}=\frac{\ell_{\mathrm P}}c=\sqrt{\frac{\hbar G}{c^5}}',
        'A light signal in vacuum traverses one Planck length in one Planck time. The fifth power of c follows from converting the length scale to time before taking the square root.'),
    'PlanckMass': (
        r'm_{\mathrm P}=\sqrt{\frac{\hbar c}{G}},\qquad\frac{Gm_{\mathrm P}^2}{\hbar c}=1',
        'The second relation identifies the unreduced mass scale by a unit gravitational coupling. The frequently used reduced Planck mass is mP/√(8π), so conventions must agree before comparing numeric values.'),
    'PlanckMassEnergyEquivalentInGev': (
        r'E_{\mathrm P}=m_{\mathrm P}c^2=\sqrt{\frac{\hbar c^5}{G}}',
        'This entry expresses a rest-energy scale in GeV. A mass in GeV/c² and its rest energy in GeV have the same numerical value only under that explicit unit convention; the kilogram value is different.'),
    'PlanckTemperature': (
        r'T_{\mathrm P}=\frac{E_{\mathrm P}}{k_{\mathrm B}}=\frac1{k_{\mathrm B}}\sqrt{\frac{\hbar c^5}{G}}',
        'Boltzmann’s constant converts the Planck energy into kelvin. This defines an energy-equivalent temperature scale; it does not assert that an ordinary material can remain in its usual thermodynamic state at that temperature.'),
    'ElementaryChargeOverH': (
        r'\frac eh=\frac{K_{\mathrm J}}2,\qquad\nu=\frac eh V',
        'The frequency ν corresponds to the energy eV of one elementary charge across a voltage difference. A Josephson junction uses charge 2e and therefore twice this frequency-per-voltage factor.'),
    'BohrMagneton': (
        r'\mu_{\mathrm B}=\frac{e\hbar}{2m_{\mathrm e}}',
        'The positive scale uses the electron mass. An actual electron magnetic moment has a sign and a g-factor correction; setting it equal to this positive scale would discard that physical information.'),
    'NuclearMagneton': (
        r'\mu_{\mathrm N}=\frac{e\hbar}{2m_{\mathrm p}},\qquad\frac{\mu_{\mathrm N}}{\mu_{\mathrm B}}=\frac{m_{\mathrm e}}{m_{\mathrm p}}',
        'Replacing electron mass by the much larger proton mass makes this magnetic-moment scale smaller. A nucleus’s composition and spin determine its moment; the word nuclear does not imply that every nucleus has moment μN.'),
    'ClassicalElectronRadius': (
        r'r_{\mathrm e}=\frac{e^2}{4\pi\varepsilon_0m_{\mathrm e}c^2}=\alpha^2a_0',
        'The electrostatic coupling divided by electron rest energy defines a length. It is useful in low-energy scattering formulas, but is not an experimental measurement of a finite electron surface.'),
    'ComptonWavelength': (
        r'\lambda_{\mathrm C}=\frac{h}{m_{\mathrm e}c},\qquad\frac{hc}{\lambda_{\mathrm C}}=m_{\mathrm e}c^2',
        'A photon at this wavelength has the electron’s rest energy. It is an intrinsic mass-based scale; the de Broglie wavelength h/p instead depends on the particle’s momentum.'),
    'ComptonWavelengthOver2Pi': (
        r'\bar\lambda_{\mathrm C}=\frac{\lambda_{\mathrm C}}{2\pi}=\frac{\hbar}{m_{\mathrm e}c}',
        'The bar denotes the reduced Compton wavelength. Expressions written with angular wavenumber naturally use ℏ; replacing the ordinary wavelength by the reduced one without its factor 2π changes the numerical prediction.'),
    'InverseOfConductanceQuantum': (
        r'G_0^{-1}=\frac{h}{2e^2}=\frac{R_{\mathrm K}}2',
        'Reciprocating a conductance changes siemens to ohms. The factor two comes from the spin-degenerate conductance quantum, so this resistance is half the von Klitzing constant rather than equal to it.'),
    'MagneticFluxQuantum': (
        r'\Phi_0=\frac{h}{2e}',
        'The denominator uses the magnitude of a Cooper pair’s charge. Flux is measured in webers; multiplying by an integer counts flux quanta, while a magnetic field requires specifying the relevant area and geometry.'),
    'JosephsonConstant': (
        r'K_{\mathrm J}=\frac{2e}{h},\qquad f=K_{\mathrm J}V',
        'This relation gives ordinary oscillation frequency in hertz from a junction voltage in volts. It uses cycles per second; an angular-frequency equation needs an additional factor 2π.'),
    'VonKlitzingConstant': (
        r'R_{\mathrm K}=\frac{h}{e^2},\qquad R_{\mathrm H}=\frac{R_{\mathrm K}}n,\quad n=1,2,\ldots',
        'n labels an integer quantum Hall plateau. Increasing the plateau index divides the resistance; it does not multiply RK. The quantized relation presumes the appropriate quantum Hall state.'),
    'ConventionalValueOfJosephsonConstant': (
        r'K_{\mathrm{J-90}}=483\,597.9\times10^9\ \mathrm{Hz\,V^{-1}}',
        'The decimal was assigned exactly for the 1990 voltage convention. It describes that historical realization; the difference from the Josephson constant in SI is a unit-conversion issue, not random measurement scatter.'),
    'ConventionalValueOfVonKlitzingConstant': (
        r'R_{\mathrm{K-90}}=25\,812.807\ \Omega',
        'This exact historical assignment defined the conventional resistance realization. Measurements reported in ohm-90 and SI ohms require the corresponding conversion even though both numbers are close.'),
    'InverseFineStructureConstant': (
        r'\alpha^{-1}=\frac{4\pi\varepsilon_0\hbar c}{e^2}',
        'This dimensionless reciprocal is often quoted because it is near 137. Its large value represents the inverse of a small electromagnetic coupling, not a separate interaction strength.'),
    'FermiCouplingConstant': (
        r'g_{\mathrm F}=\frac{G_{\mathrm F}}{(\hbar c)^3},\qquad[g_{\mathrm F}]=\mathrm{GeV}^{-2}',
        'gF denotes the energy-unit representation stored here. Its inverse-energy-squared dimension belongs to the low-energy four-fermion description of weak interactions; it is not a dimensionless probability or a Newtonian gravitational coupling.'),
    'WeakMixingAngle': (
        r'\sin^2\theta_{\mathrm W}=1-\left(\frac{m_{\mathrm W}}{m_{\mathrm Z}}\right)^2\quad\text{(on-shell convention)}',
        'mW and mZ are the boson masses in the stated convention. Taking asin directly of the stored number is incorrect if an angle is wanted: on the usual first-quadrant branch, take the square root before the inverse sine.'),
    'RydbergConstant': (
        r'R_\infty=\frac{\alpha^2m_{\mathrm e}c}{2h},\qquad\frac1\lambda=R_\infty\left(\frac1{n_1^2}-\frac1{n_2^2}\right)',
        'For the ideal infinite-nuclear-mass hydrogen model, n2>n1 gives a positive emission wavenumber. This is inverse wavelength, not frequency. Finite nuclear mass and higher-order atomic corrections modify actual spectral lines.'),
    'RydbergConstantTimesCInHz': (
        r'\nu_{\mathrm R}=cR_\infty,\qquad E_{\mathrm R}=h\nu_{\mathrm R}',
        'Multiplying a spectroscopic wavenumber in inverse meters by c in meters per second gives hertz. There is no 2π here because R∞ is an ordinary wavenumber, not an angular one.'),
    'RydbergConstantTimesHcInJ': (
        r'E_{\mathrm R}=hcR_\infty=\frac{E_{\mathrm h}}2',
        'This is the positive Rydberg energy scale. In the ideal hydrogen model the ground-state energy relative to the ionization threshold is −ER; the required ionization energy is positive ER.'),
    'RydbergConstantTimesHcInEv': (
        r'E_{\mathrm R}[\mathrm{eV}]=\frac{E_{\mathrm h}[\mathrm{eV}]}2',
        'Both energies in this relation are numerical values in electronvolts. A factor one half relates Rydberg and Hartree conventions; unit conversion from joules must be applied consistently to both.'),
    'HartreeEnergy': (
        r'E_{\mathrm h}=\alpha^2m_{\mathrm e}c^2=\frac{\hbar^2}{m_{\mathrm e}a_0^2}=2hcR_\infty',
        'Hartree atomic units choose this energy as one unit. The ideal hydrogen ground-state energy is −Eh/2, so one Hartree is twice the positive ionization-energy scale of that model.'),
    'HartreeEnergyInEv': (
        r'E_{\mathrm h}[\mathrm{eV}]=2E_{\mathrm R}[\mathrm{eV}]',
        'The physical Hartree energy is unchanged; this entry expresses it in electronvolts. Multiplying a number of Hartrees by this factor returns eV directly, while the HartreeEnergy entry instead returns joules.'),
    'ThomsonCrossSection': (
        r'\sigma_{\mathrm T}=\frac{8\pi}{3}r_{\mathrm e}^{\,2},\qquad E_\gamma\ll m_{\mathrm e}c^2',
        'Squaring the classical radius gives the area dimension of a cross section. The displayed low-photon-energy condition identifies the Thomson limit; photons comparable to electron rest energy require a recoil-aware scattering description.'),
    'QuantumOfCirculation': (
        r'\kappa_{\mathrm{pair}}=\frac{h}{2m_{\mathrm e}}',
        'Dividing action by mass gives units of square meters per second. The mass in this particular scale is an electron-pair mass; applying it to another condensate without replacing the mass gives the wrong circulation scale.'),
    'QuantumOfCirculationTimes2': (
        r'\kappa_{\mathrm e}=\frac{h}{m_{\mathrm e}}=2\kappa_{\mathrm{pair}}',
        'The factor two changes the mass convention from 2me to me. It does not specify a flow speed by itself: relating a circulation to a speed also requires a path length and flow model.'),
    'ProtonMagneticShieldingCorrection': (
        r'\sigma_{\mathrm p}=1-\frac{\mu_{\mathrm p}^{\prime}}{\mu_{\mathrm p}},\qquad\mu_{\mathrm p}^{\prime}=\mu_{\mathrm p}(1-\sigma_{\mathrm p})',
        'The prime marks the shielded reference proton moment. A positive shielding fraction reduces that moment relative to the free proton under the stated convention; it is not a correction in teslas.'),
    'AtomicMassConstantEnergyEquivalent': (
        r'E_{\mathrm u}=m_{\mathrm u}c^2,\qquad m_{\mathrm u}=\frac{m({}^{12}\mathrm C)}{12}',
        'An atomic mass constant is a mass, while this entry is its rest-energy equivalent in joules. Dividing a rest energy by this scale yields its mass equivalent in unified atomic mass units.'),
    'AtomicMassConstantEnergyEquivalentInMeV': (
        r'E_{\mathrm u}=m_{\mathrm u}c^2,\qquad E[\mathrm{MeV}]=m[\mathrm u]\,E_{\mathrm u}[\mathrm{MeV}]',
        'A mass expressed as a number of unified atomic mass units becomes rest energy in MeV after multiplication by this factor. Nuclear binding energies involve mass differences; summing free-particle energies omits binding.'),
    'BoltzmannConstantInHzperK': (
        r'\frac{\nu_T}{T}=\frac{k_{\mathrm B}}h,\qquad h\nu_T=k_{\mathrm B}T',
        'This defines a frequency whose photon energy equals the thermal scale kBT. A thermal spectrum contains many frequencies, so νT is an energy equivalence rather than a statement that every thermal photon has that frequency.'),
    'BoltzmannConstantInInverseMetersPerKelvin': (
        r'\frac{\widetilde\nu_T}{T}=\frac{k_{\mathrm B}}{hc},\qquad\widetilde\nu_T=\frac1{\lambda_T}',
        'The output is spectroscopic wavenumber in inverse meters per kelvin. Angular wavenumber is 2π times this value; the distinction matters when combining the result with ℏc.'),
    'FaradayConstantForConventionalElectricCurrent': (
        r'F_{90}=\frac{2N_{\mathrm A}}{K_{\mathrm{J-90}}R_{\mathrm{K-90}}}',
        'The 1990 Josephson and von Klitzing assignments determine the conventional charge unit, so this expression converts a mole of elementary charges to C90. F90 here names a Faraday coefficient; it is not the conventional unit called farad-90.'),
    'FirstRadiationConstant': (
        r'c_1=2\pi hc^2,\qquad M_\lambda=\frac{c_1}{\lambda^5\left(e^{c_2/(\lambda T)}-1\right)}',
        'Mλ is blackbody spectral exitance per unit wavelength, integrated over the outgoing hemisphere. The factor π follows from angular integration of isotropic radiance with its projected-area factor; it is not an extra wavelength conversion.'),
    'FirstRadiationConstantForSpectralRadiance': (
        r'c_{1L}=2hc^2,\qquad B_\lambda=\frac{c_{1L}}{\lambda^5\left(e^{c_2/(\lambda T)}-1\right)}',
        'Bλ remains a radiance per unit solid angle and wavelength. In the blackbody setting, hemispherical exitance is πBλ; multiplying by 4π would incorrectly omit the projected-area weighting.'),
    'SecondRadiationConstant': (
        r'c_2=\frac{hc}{k_{\mathrm B}},\qquad x=\frac{c_2}{\lambda T}',
        'The Planck-law exponent x must be dimensionless. With wavelength in meters and temperature in kelvin, c2 has units meter-kelvin; using nanometers without conversion changes the exponent by a billion.'),
    'MolarMassConstant': (
        r'M_{\mathrm u}=N_{\mathrm A}m_{\mathrm u},\qquad M(X)=A_{\mathrm r}(X)M_{\mathrm u}',
        'Relative atomic mass Ar is dimensionless. Multiplying it by the molar mass constant gives kilograms per mole, whereas multiplying by the atomic mass constant gives the mass of one atom.'),
    'MolarMassOfCarbon12': (
        r'M({}^{12}\mathrm C)=12M_{\mathrm u}=12N_{\mathrm A}m_{\mathrm u}',
        'The relation refers to free carbon-12 atoms in their ground state. Its factor twelve follows from the definition of atomic mass; the modern kilogram-per-mole value of Mu is still subject to the metrology qualification above.'),
    'MolarPlanckConstant': (
        r'H_{\mathrm m}=N_{\mathrm A}h,\qquad E_{\mathrm m}=H_{\mathrm m}\nu',
        'Hm denotes the molar Planck coefficient and Em an energy per mole of quanta. The factor NA changes a per-photon energy to a molar energy; frequency remains ordinary frequency in hertz.'),
    'MolarPlanckConstantTimesC': (
        r'E_{\mathrm m}=\frac{N_{\mathrm A}hc}{\lambda}',
        'Dividing the stored joule-meter-per-mole factor by a photon wavelength in meters gives joules per mole. Using an inverse wavelength instead requires multiplication, and using nanometers first requires a length conversion.'),
    'AngstromStar': (
        r'\lambda(\mathrm W\,K\alpha_1)=0.2090100\,\mathring{\mathrm A}^{*}',
        'The tungsten Kα1 wavelength defines this historical unit by the assigned multiplier. The star is part of the unit’s name; dropping it substitutes the exact ångström and loses the calibration distinction.'),
    'CuXUnit': (
        r'\lambda(\mathrm{Cu}\,K\alpha_1)=1537.400\,\mathrm{xu}(\mathrm{Cu})',
        'The copper line is assigned this many copper X units. Converting the unit to meters requires a measured wavelength; the assigned line multiplier itself does not make the meter conversion exact.'),
    'MoXUnit': (
        r'\lambda(\mathrm{Mo}\,K\alpha_1)=707.831\,\mathrm{xu}(\mathrm{Mo})',
        'The assigned molybdenum line multiplier is 707.831. Copper and molybdenum X units were calibrated separately, so the same number of X units does not imply exactly the same length for both labels.'),
    'LatticeParameterOfSilicon': (
        r'a=\sqrt8\,d_{220},\qquad V_{\mathrm{cell}}=a^3',
        'a is the conventional cubic cell edge, not the distance between {220} planes. Cubing the edge gives cell volume; comparison with a molar volume must also count the eight silicon atoms in a diamond-cubic conventional cell.'),
    '_220LatticeSpacingOfSilicon': (
        r'd_{hkl}=\frac{a}{\sqrt{h^2+k^2+l^2}},\qquad d_{220}=\frac{a}{\sqrt8}',
        'h, k and l are Miller indices for the cubic lattice in this equation. Substituting 2, 2 and 0 produces √8 in the denominator; the spacing is not a/2 and depends on the reference crystal conditions.'),
    'MolarVolumeOfSilicon': (
        r'V_{\mathrm m}=N_{\mathrm A}\frac{a^3}{8}',
        'Eight atoms occupy one conventional diamond-cubic cell, so a³/8 is volume per atom. Multiplication by Avogadro’s constant gives volume per mole of atoms; omitting eight would give a volume eight times too large.'),
}
