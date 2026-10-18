const { test } = require('node:test');
const assert = require('node:assert/strict');
const { models, contours } = require('./plots.cjs');

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
