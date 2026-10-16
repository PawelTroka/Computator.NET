"""Reviewed equations for the closed physical-constant families in physical.py.

These are dimensional definitions and conversion rules, not formulas inferred
from stored decimals or arbitrary prose. Numerical values remain in the original
catalogue, including their historical metrology qualifications.
"""
import physical

ATOMIC_TEX = {
    '1stHyperpolarizability': r'\frac{e^3a_0^3}{E_{\mathrm h}^2}',
    '2ndHyperpolarizability': r'\frac{e^4a_0^4}{E_{\mathrm h}^3}',
    'Action': r'\hbar', 'Charge': 'e', 'ChargeDensity': r'\frac{e}{a_0^3}',
    'Current': r'\frac{eE_{\mathrm h}}{\hbar}',
    'ElectricDipoleMoment': r'ea_0', 'ElectricField': r'\frac{E_{\mathrm h}}{ea_0}',
    'ElectricFieldGradient': r'\frac{E_{\mathrm h}}{ea_0^2}',
    'ElectricPolarizability': r'\frac{e^2a_0^2}{E_{\mathrm h}}',
    'ElectricPotential': r'\frac{E_{\mathrm h}}e', 'ElectricQuadrupoleMoment': r'ea_0^2',
    'Energy': r'E_{\mathrm h}', 'Force': r'\frac{E_{\mathrm h}}{a_0}', 'Length': r'a_0',
    'MagneticDipoleMoment': r'\frac{e\hbar}{m_e}=2\mu_{\mathrm B}',
    'MagneticFluxDensity': r'\frac{\hbar}{ea_0^2}',
    'Magnetizability': r'\frac{e^2a_0^2}{m_e}', 'Mass': r'm_e',
    'Momentum': r'\frac{\hbar}{a_0}',
    'Permittivity': r'\frac{e^2}{a_0E_{\mathrm h}}=4\pi\varepsilon_0',
    'Time': r'\frac{\hbar}{E_{\mathrm h}}', 'Velocity': r'\alpha c',
}
NATURAL_TEX = {
    'Action': r'\hbar', 'ActionInEvS': r'\hbar',
    'Energy': r'm_ec^2', 'EnergyInMeV': r'm_ec^2',
    'Length': r'\frac{\hbar}{m_ec}', 'Mass': r'm_e',
    'Momentum': r'm_ec', 'MomentumInMeVperC': r'm_ec',
    'Time': r'\frac{\hbar}{m_ec^2}', 'Velocity': 'c',
}

# Each second component is the energy of exactly one named source/target unit.
# Their quotient converts numerical values, so no physical unit is silently lost.
ENERGY_UNITS = {
    'AtomicMassUnit': (r'\mathrm u', r'm_{\mathrm u}c^2'),
    'ElectronVolt': (r'\mathrm{eV}', r'e\,(1\,\mathrm V)'),
    'Hartree': (r'E_{\mathrm h}', r'E_{\mathrm h}'),
    'Hertz': (r'\mathrm{Hz}', r'h\,(1\,\mathrm{Hz})'),
    'InverseMeter': (r'\mathrm{m}^{-1}', r'hc\,(1\,\mathrm{m}^{-1})'),
    'Joule': (r'\mathrm J', r'1\,\mathrm J'),
    'Kelvin': (r'\mathrm K', r'k_{\mathrm B}\,(1\,\mathrm K)'),
    'Kilogram': (r'\mathrm{kg}', r'(1\,\mathrm{kg})c^2'),
}
PARTICLE_SYMBOLS = {
    'AlphaParticle': (r'm_{\alpha}', r'\mu_{\alpha}', r'\alpha'),
    'Electron': (r'm_e', r'\mu_e', 'e'),
    'Muon': (r'm_{\mu}', r'\mu_{\mu}', r'\mu'),
    'Tau': (r'm_{\tau}', r'\mu_{\tau}', r'\tau'),
    'Proton': (r'm_p', r'\mu_p', 'p'),
    'Neutron': (r'm_n', r'\mu_n', 'n'),
    'Deuteron': (r'm_d', r'\mu_d', 'd'),
    'Triton': (r'm_t', r'\mu_t', 't'),
    'Helion': (r'm_{\mathrm{hel}}', r'\mu_{\mathrm{hel}}', r'\mathrm{hel}'),
    'ShieldedProton': (r'm_p', r'\mu_p^{\prime}', r'p^{\prime}'),
    'ShieldedHelion': (r'm_{\mathrm{hel}}', r'\mu_{\mathrm{hel}}^{\prime}', r'\mathrm{hel}^{\prime}'),
}


def family_lesson(name):
    """Return (TeX, interpretation paragraphs), or None for a named base constant."""
    name = physical.ALIASES.get(name, name)
    if name in physical.BASE:
        return None
    if name.startswith('AtomicUnitOf'):
        key = name[len('AtomicUnitOf'):]
        tex = r'\mathrm{a.u.}(\text{' + physical.ATOMIC[key][0] + '})=' + ATOMIC_TEX[key]
        paragraphs = {
            'MagneticDipoleMoment': 'This atomic unit is twice the Bohr magneton. Using one Bohr magneton as the conversion factor would halve the stated moment.',
            'Permittivity': 'The Hartree convention absorbs 4π with vacuum permittivity. Replacing this scale by ε₀ alone would mix two electromagnetic unit conventions.',
            'ElectricFieldGradient': 'A field gradient changes electric field per unit distance, so its denominator contains two powers of the Bohr radius. It is not the same unit as an electric field.',
            '1stHyperpolarizability': 'The first hyperpolarizability describes a second-order electric-field response. Its unit therefore differs from the linear polarizability; the tensor and factorial convention must still be supplied by the model.',
            '2ndHyperpolarizability': 'The second hyperpolarizability describes a third-order electric-field response. Its dimensional scale alone does not specify the tensor components or the factorial convention in a response expansion.',
        }
        return tex, (paragraphs[key],) if key in paragraphs else ()
    if name.startswith('NaturalUnitOf'):
        key = name[len('NaturalUnitOf'):]
        return (r'u_{\mathrm n}(\text{' + physical.NATURAL[key][0] + '})=' + NATURAL_TEX[key], ())
    if name.endswith('Relationship'):
        middle = name[:-len('Relationship')]
        pairs = [(a, b) for a in ENERGY_UNITS for b in ENERGY_UNITS if a != b and a + b == middle]
        if len(pairs) != 1:
            raise KeyError('Unknown energy conversion: ' + name)
        a, b = pairs[0]
        ua, ea = ENERGY_UNITS[a]; ub, eb = ENERGY_UNITS[b]
        tex = r'q_{' + ub + r'}=q_{' + ua + r'}\,\frac{' + ea + '}{' + eb + '}'
        return tex, ('The q symbols denote numerical values in the indicated units. The fraction divides the energy of one source unit by that of one target unit; physical e denotes ElementaryCharge.',)
    if name.startswith(('LoschmidtConstant', 'MolarVolumeOfIdealGas')):
        pressure = r'101\,325' if '101325' in name else r'100\,000'
        equation = r'n_0=\frac{p}{k_{\mathrm B}T}' if name.startswith('Loschmidt') else r'V_{\mathrm m}=\frac{RT}{p}'
        return equation + r',\qquad T=273.15\,\mathrm K,\quad p=' + pressure + r'\,\mathrm{Pa}', ()
    if name.startswith('SackurTetrodeConstant'):
        pressure = r'101\,325' if '101325' in name else r'100\,000'
        return (r'\frac{S_0}{R}=\frac52+\ln\left[\frac{(2\pi m_{\mathrm u}k_{\mathrm B}T_0)^{3/2}k_{\mathrm B}T_0}{h^3p_0}\right],'
                r'\quad T_0=1\,\mathrm K,\quad p_0=' + pressure + r'\,\mathrm{Pa}',
                ('The logarithm’s argument is dimensionless: its numerator and denominator have matching units. The reference mass is the atomic mass constant, rather than the mass of whichever gas happens to be studied.',))
    if name in ('FirstChandrasekharConstant', 'SecondChandrasekharConstant'):
        tex = (r'\operatorname E[r]=\frac{\Gamma(4/3)}{(4\pi/3)^{1/3}}\,n^{-1/3}'
               if name.startswith('First') else r'r_{\mathrm{mode}}=\left(\frac1{2\pi}\right)^{1/3}n^{-1/3}')
        return tex, ('For a spatial Poisson process, the nearest-neighbor distance is random. Its mean and its most probable value need not coincide; increasing density by eight divides both length scales by two.',)
    for root, symbol in [('BohrMagneton', r'\mu_{\mathrm B}'), ('NuclearMagneton', r'\mu_{\mathrm N}')]:
        if name.startswith(root + 'In'):
            key = name[len(root + 'In'):]
            denominator = {'EvperT': r'e\,(1\,\mathrm V)', 'HzperT': 'h',
                           'MhzperT': r'10^6h', 'InverseMetersPerTesla': 'hc', 'KperT': r'k_{\mathrm B}'}[key]
            return r'\frac{' + symbol + '}{' + denominator + '}', ()
    if name.startswith('NeutronProtonMassDifference'):
        key = name[len('NeutronProtonMassDifference'):]
        tex = {'': r'\Delta m=m_n-m_p', 'InU': r'\frac{\Delta m}{m_{\mathrm u}}=\frac{m_n-m_p}{m_{\mathrm u}}',
               'EnergyEquivalent': r'\Delta E=(m_n-m_p)c^2',
               'EnergyEquivalentInMeV': r'\frac{\Delta E}{1\,\mathrm{MeV}}=\frac{(m_n-m_p)c^2}{10^6e\,(1\,\mathrm V)}'}[key]
        return tex, ('Use the masses of the free neutron and proton. The mass difference between two nuclei also includes binding-energy changes and cannot be obtained merely by counting their neutrons.',)
    for particle in sorted(PARTICLE_SYMBOLS, key=len, reverse=True):
        if not name.startswith(particle):
            continue
        rest = name[len(particle):]
        if rest.startswith('To'):
            rest = rest[2:]
        mass, moment, symbol = PARTICLE_SYMBOLS[particle]
        for partner in sorted(PARTICLE_SYMBOLS, key=len, reverse=True):
            mass2, moment2, _ = PARTICLE_SYMBOLS[partner]
            if rest == partner + 'MassRatio':
                return r'R=\frac{' + mass + '}{' + mass2 + '}', ()
            if rest == partner + 'MagneticMomentRatio':
                return r'R_\mu=\frac{' + moment + '}{' + moment2 + '}', ()
        mass_tex = {
            'Mass': mass + r'=\frac{E_0}{c^2}',
            'MassInU': r'A_{\mathrm r}=\frac{' + mass + r'}{m_{\mathrm u}}',
            'MassEnergyEquivalent': r'E_0=' + mass + r'c^2',
            'MassEnergyEquivalentInMeV': r'\frac{E_0}{1\,\mathrm{MeV}}=\frac{' + mass + r'c^2}{10^6e\,(1\,\mathrm V)}',
            'MolarMass': r'M=N_{\mathrm A}' + mass,
            'ComptonWavelength': r'\lambda_{\mathrm C}=\frac{h}{' + mass + 'c}',
            'ComptonWavelengthOver2Pi': r'\frac{\lambda_{\mathrm C}}{2\pi}=\frac{\hbar}{' + mass + 'c}',
        }
        if rest in mass_tex:
            return mass_tex[rest], (('E₀ denotes rest energy. Kinetic energy from motion is additional; substituting total energy into this relation would not recover the invariant rest mass.',) if rest == 'Mass' else ())
        if rest == 'ChargeToMassQuotient':
            charge = {'Electron': '-e', 'Proton': 'e'}[particle]
            return r'\frac{q}{m}=\frac{' + charge + '}{' + mass + '}', ()
        if rest == 'MagneticMoment':
            return r'\Delta E=-\vec{\mu}\cdot\mathbf B,\qquad \mu=' + moment, ()
        if rest in ('MagneticMomentToBohrMagnetonRatio', 'MagneticMomentToNuclearMagnetonRatio'):
            denominator = r'\mu_{\mathrm B}' if 'Bohr' in rest else r'\mu_{\mathrm N}'
            return r'R_\mu=\frac{' + moment + '}{' + denominator + '}', ()
        if rest == 'GFactor':
            spin = '1' if particle == 'Deuteron' else r'\frac12'
            reference = r'\frac{e\hbar}{2' + mass + '}' if particle in ('Electron', 'Muon', 'Tau') else r'\mu_{\mathrm N}=\frac{e\hbar}{2m_p}'
            return moment + r'=g_{' + symbol + r'}\mu_{\mathrm{ref}}I,\quad \mu_{\mathrm{ref}}=' + reference + r',\quad I=' + spin, ()
        if rest == 'MagneticMomentAnomaly':
            return r'a_{' + symbol + r'}=\frac{|g_{' + symbol + r'}|-2}{2}', ()
        if rest.startswith('GyromagneticRatio'):
            tex = (r'\frac{|\gamma|/(2\pi)}{1\,\mathrm{MHz}\,\mathrm T^{-1}}=\frac{|' + moment + r'|/(1\,\mathrm J\,\mathrm T^{-1})}{10^6\,[h/(1\,\mathrm J\,\mathrm s)]\,I}'
                   if rest.endswith('Over2Pi') else r'|\gamma|=\frac{|' + moment + r'|}{\hbar I}')
            return tex + r',\qquad I=\frac12', ()
        if rest == 'RmsChargeRadius':
            return r'r_{\mathrm{ch}}=\sqrt{\langle r^2\rangle_{\mathrm{ch}}}', ()
    raise KeyError('No reviewed physical-family equation for ' + name)
