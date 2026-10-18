const { test } = require('node:test');
const assert = require('node:assert/strict');
const { models, contours, diagrams, unitCircleGeometry, normalSamples } = require('./plots.cjs');

const close = (actual, expected, tolerance = 1e-10) => assert.ok(Math.abs(actual-expected) <= tolerance,
  `Expected ${expected}, received ${actual}`);

test('benchmark models preserve analytic values and the actual Bohachevsky offset', () => {
  close(models.Ackley.value(0,0),0);
  close(models.Ackley.value(1,1),20*(1-Math.exp(-0.2)));
  close(models.Rastrigin.value(1,2),5);
  close(models.Rastrigin.value(0.5,0),20.25);
  close(models.Bohachevsky1.value(0,0),-0.7);
  close(models.Bohachevsky1.value(1,0),0.9);
  close(models.Himmelblau.value(3,2),0);
  close(models.Himmelblau.value(0,0),170);
  close(models.Matyas.value(2,2),0.16);
  close(models.Matyas.value(2,-2),4);
  close(models.Rosenbrock.value(1,1),0);
  close(models.Rosenbrock.value(0,1),101);
  close(models.dropWave.value(0,0),-1);
  close(models.sixHumpCamel.value(0,0),0);
  close(models.sixHumpCamel.value(1,0),67/30);
});

test('plotted paths explain the two coupled benchmark valleys', () => {
  for (const x of [-1.7,-0.5,0,0.8,1,1.6]) {
    close(models.Matyas.value(x,models.Matyas.slice(x)),0.04*x*x);
    close(models.Rosenbrock.value(x,models.Rosenbrock.slice(x)),(1-x)**2);
  }
});

test('global-minimum markers use the documented coordinates and objective values', () => {
  const minima = { Ackley: 0, Rastrigin: 0, Bohachevsky1: -0.7, Himmelblau: 0,
    Matyas: 0, Rosenbrock: 0, dropWave: -1, sixHumpCamel: -1.03162845349 };
  for (const [name,model] of Object.entries(models)) {
    for (const [x,y] of model.minima) {
      assert.ok(x >= model.x[0] && x <= model.x[1] && y >= model.y[0] && y <= model.y[1],name);
      close(model.value(x,y),minima[name],1e-8);
    }
  }
  assert.equal(models.Himmelblau.minima.length,4);
  assert.equal(models.sixHumpCamel.minima.length,2);
});

test('contours trace a circle at the requested value and remain inside the data bounds', () => {
  const circle = contours((x,y) => x*x+y*y,[-2,2],[-2,2],1,80);
  assert.ok(circle.length > 100);
  for (const segment of circle) for (const [x,y] of segment) {
    assert.ok(x >= -1 && x <= 1 && y >= -1 && y <= 1);
    close(x*x+y*y,1,0.0013);
  }
  assert.deepEqual(contours(() => 0,[-1,1],[-1,1],1,8),[]);
});

test('saddle contours do not connect through a region with the wrong value', () => {
  const saddle = contours((x,y) => x*y,[-1,1],[-1,1],0.2,80);
  assert.ok(saddle.length > 0);
  for (const [a,b] of saddle) {
    close(a[0]*a[1],0.2);
    close(b[0]*b[1],0.2);
    assert.equal(Math.sign(a[0]),Math.sign(b[0]));
    assert.equal(Math.sign(a[1]),Math.sign(b[1]));
  }
});

test('unit-circle geometry gives the exact real pi/4 projections and radius', () => {
  const model = diagrams.UnitCircle, geometry = unitCircleGeometry();
  close(model.angle,Math.PI/4);
  assert.equal(model.radius,1);
  close(geometry.point[0],Math.SQRT1_2);
  close(geometry.point[1],Math.SQRT1_2);
  close(geometry.point[0]**2 + geometry.point[1]**2,1);
  close(Math.atan2(geometry.point[1],geometry.point[0]),model.angle);
  assert.deepEqual(geometry.cosine,[[0,0],[geometry.point[0],0]]);
  assert.deepEqual(geometry.sine,[[geometry.point[0],0],geometry.point]);
  assert.deepEqual(geometry.horizontal,[[0,geometry.point[1]],geometry.point]);
  for (const [x,y] of geometry.arc) close(x*x+y*y,0.28**2);
  close(Math.atan2(...geometry.arc.at(-1).toReversed()),model.angle);
  assert.match(model.caption,/Real-angle illustration/);
  assert.match(model.caption,/Complex inputs are not represented/);
});

test('projections retain signed coordinates in all real-angle quadrants', () => {
  const cases = [[0,1,0], [Math.PI/2,0,1], [Math.PI,-1,0],
    [3*Math.PI/2,0,-1], [-Math.PI/4,Math.SQRT1_2,-Math.SQRT1_2]];
  for (const [angle,x,y] of cases) {
    const geometry = unitCircleGeometry(angle);
    close(geometry.point[0],x); close(geometry.point[1],y);
    close(geometry.cosine[1][0],x);
    close(geometry.sine[1][1],y);
    close(geometry.sine[0][0],geometry.sine[1][0]);
  }
  for (const invalid of [NaN,Infinity,-Infinity,'0']) assert.throws(() => unitCircleGeometry(invalid));
});

test('standard-normal density uses sigma one and has the correct peak and symmetry', () => {
  const model = diagrams.StandardNormal;
  assert.equal(model.mean,0); assert.equal(model.stddev,1); assert.equal(model.threshold,1);
  assert.deepEqual(model.x,[-4,4]);
  close(model.value(0),0.3989422804014327);
  close(model.value(1),0.24197072451914337);
  let previous = Infinity;
  for (const x of [0,0.25,0.5,1,2,3,4]) {
    close(model.value(x),model.value(-x));
    assert.ok(model.value(x) > 0 && model.value(x) < previous);
    previous = model.value(x);
  }
  assert.ok(Math.abs(model.value(1)-model.probability) > 0.5,'Density height must not be labelled as cumulative probability.');
});

test('normal shading ends exactly at one even when the threshold lies between samples', () => {
  const model = diagrams.StandardNormal;
  for (const segments of [800,503]) {
    const { curve, shaded } = normalSamples(segments);
    assert.equal(curve.length,segments+1);
    assert.equal(curve[0][0],-4); assert.equal(curve.at(-1)[0],4);
    assert.equal(shaded[0][0],-4); assert.equal(shaded.at(-1)[0],1);
    assert.ok(shaded.every(([x,y]) => x <= 1 && y > 0 && y < model.y[1]));
    for (let i = 0; i < curve.length; i++) {
      close(curve[i][1],model.value(curve[i][0]));
      if (i) assert.ok(curve[i][0] > curve[i-1][0]);
    }
    close(shaded.at(-1)[1],0.24197072451914337);
  }
  for (const invalid of [0,7,8.5,10001,NaN]) assert.throws(() => normalSamples(invalid));
});

test('independent density integration verifies the labelled probability and omitted tails', () => {
  // Simpson integration is independent of both the fixed probability annotation
  // and the uniformly sampled polygon used by the renderer.
  function integrate(value,a,b) {
    const count = 20000, step = (b-a)/count;
    let sum = value(a)+value(b);
    for (let i = 1; i < count; i++) sum += (i%2 ? 4 : 2)*value(a+i*step);
    return sum*step/3;
  }
  const model = diagrams.StandardNormal;
  close(integrate(model.value,-10,10),1,1e-12);
  close(integrate(model.value,-10,0),0.5,1e-12);
  close(integrate(model.value,-10,model.threshold),model.probability,1e-12);
  const omittedLeftTail = integrate(model.value,-10,model.x[0]);
  close(omittedLeftTail,0.000031671241833119924,1e-14);
  close(integrate(model.value,model.x[0],model.threshold)+omittedLeftTail,model.probability,1e-12);
  assert.equal(model.probability.toFixed(4),'0.8413');
  assert.match(model.caption,/both tails continue beyond the plotted range/);
});
