#include <math.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <gsl/gsl_version.h>
#include <gsl/gsl_errno.h>
#include <gsl/gsl_sf_gamma.h>
#include <gsl/gsl_sf_bessel.h>
#include <gsl/gsl_cdf.h>
#include <gsl/gsl_blas.h>

static int close_to(double actual, double expected, double tolerance) {
    return isfinite(actual) && fabs(actual - expected) <= tolerance;
}
int main(void) {
    gsl_sf_result result;
    gsl_set_error_handler_off();
    if (strcmp(gsl_version, "2.8") != 0) return 1;
    if (sizeof(gsl_sf_result) != 16 || offsetof(gsl_sf_result, err) != 8) return 2;
    if (gsl_sf_gamma_e(3.0, &result) != GSL_SUCCESS || !close_to(result.val, 2.0, 1e-13)) return 3;
    if (gsl_sf_gamma_e(0.0, &result) != GSL_EDOM || !isnan(result.val)) return 4;
    if (gsl_sf_bessel_J0_e(0.0, &result) != GSL_SUCCESS || !close_to(result.val, 1.0, 1e-14)) return 5;
    if (!close_to(gsl_cdf_ugaussian_P(0.0), 0.5, 1e-14)) return 6;
    double a[] = { 1.0, 2.0, 3.0 }, b[] = { 4.0, 5.0, 6.0 }, product;
    gsl_vector_view av = gsl_vector_view_array(a, 3), bv = gsl_vector_view_array(b, 3);
    if (gsl_blas_ddot(&av.vector, &bv.vector, &product) != GSL_SUCCESS || product != 32.0) return 7;
    printf("GSL %s: ABI layout, gamma/domain error, Bessel, normal CDF, CBLAS passed.\n", gsl_version);
    return 0;
}
