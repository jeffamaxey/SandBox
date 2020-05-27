# -*- coding: utf-8 -*-
"""
Created on Sun Jun 30 16:23:06 2019

@author: mhoc
"""
import numpy as np
from numpy import linalg as la
from numpy import dot, cumsum, sqrt, outer, diag, exp, max, zeros, roll
from scipy.stats import norm


class EconomicScenarioGenerator(object):
    """
    """

    def __init__(self, s0, a, mu, sdev, corr):
        """
        Initialize a Scenario Generator.
        Inputs:
        - s0: Array Initial values
        - a: Speed of reversion in Vasicek
        - mu: Array Mean returns (yearly)
        - cov: Array Covariance (yearly)
        """
        ############################################################################
        # TODO: Validate dimensions in model                                                  #
        ############################################################################
        self.s0 = s0
        self.a = a
        self.mu = mu
        self.sigma = sdev
        self.Corr = corr
        self.stock_id = np.where(a == 0.0)[0]
        self.rate_id = np.where(a != 0.0)[0]
        # np.random.seed(905)

    def get_scenarios(self, N, steps, partition=252):
        """
        Generate Geometric Brownian motions or Mean Reversion w Additive Noice (Vasicek)

        Inputs:
        - N: Integer Number of simulations
        - steps: Integer Number of steps in each simulation
        - partion: Integer Parts of a year, 252 => daily
        """
        scaled_sigma = self.sigma / sqrt(partition)

        dt = 1 / partition
        S = zeros((N, len(self.stock_id), steps))
        R = zeros((N, len(self.rate_id), steps))
        for n in np.arange(N):
            dB, _ = self.brown(self.Corr, steps)
            S[n] = self.gbm(self.s0[self.stock_id], self.mu[self.stock_id], scaled_sigma[self.stock_id],
                            dt, dB[self.stock_id, 1:])
            R[n] = self.vasicek(self.s0[self.rate_id], self.a[self.rate_id], self.mu[self.rate_id],
                                scaled_sigma[self.rate_id], dt, dB[self.rate_id, 1:])
            # Kolla dB och B går i takt!
        return (S, R)

    # Brownian Motion
    def brown(self, corr, steps):
        """
        Inputs:
        - corr: Correlation matrix
        - steps: integer, number of steps
        Outputs:
        - dB: Brownian steps of shape (instruments, steps)
        - B: Brownian motion of shape (instruments, steps)
        """
        dB = self.brown_steps(corr, steps)

        # B(0) = 0
        dB[:, -1] = zeros(dB.shape[0])
        B = cumsum(roll(dB, 1, axis=1), axis=1)

        return (dB, B)

    # Geometric Brownian Motion

    def gbm(self, s0, mu, sigma, dt, dB):
        """
        Inputs:
        - s0: Array of input data of shape (N, d_1, ..., d_k)
        - mu:
        - sdev:
        - dt:
        - dB:
        """
        s = s0.copy()
        S = zeros((dB.shape[0], dB.shape[1] + 1))
        S[:, 0], n = s, 1

        for db in dB.T:
            s += mu * s * dt + sigma * s * db
            S[:, n] = s
            n += 1

        return S

    # Mean reversion rate model

    def vasicek(self, r0, a, mu, sigma, dt, dB):
        """
        Inputs:
        - r0: Array of input data of shape (N, d_1, ..., d_k)
        - a: Array of labels, of shape (N,). y[i] gives the label for X[i].
        - mu:
        - sdev:
        - dB:
        """

        r = r0.copy()
        R = zeros((dB.shape[0], dB.shape[1] + 1))
        R[:, 0], n = r, 1

        for db in dB.T:
            r += a * (mu - r) * dt + sigma * db
            R[:, n] = r
            n += 1

        return R

    # Brownian Step

    def brown_steps(self, corr, steps):
        """
        Generates [X1, X2,...,Xn]
        where X ~ N(0,cor^0.5)

        Inputs:
        - X: Array of input data of shape (N, d_1, ..., d_k)
        - y: Array of labels, of shape (N,). y[i] gives the label for X[i].
        """

        # C ~ U(0,1)
        C = self.copula(corr, steps)
        dB = norm.ppf(C)

        return dB

    def copula(self, Corr, n):
        """
        Gaussian Copula generator.

        Inputs:
        - Corr: Correlation matrix
        - n: Integer, number of samples
        """
        ############################################################################
        # TODO: Non elliptic copula                                                #
        ############################################################################

        # dot(B,B.T) = Corr
        B = self.left_factorize(Corr)
        r = B.shape[1]  # Truncated rank

        Z = np.random.standard_normal(n * r).reshape(-1, n)
        X = dot(B, Z)
        C = norm.cdf(X)

        return C

    # Singular Value Decomposition
    def left_factorize(self, A, tol=1e-6):
        """
        Factors A via truncated SVD such that

        A = B * B.T

        Inputs:
        - A: any symmetric matrix
        - tol: Scalar, use left-singular vectors corresponding to singular value > tol
        """
        U, Sigma, _ = la.svd(A)
        r = max(np.where(Sigma > tol))  # rank
        B = dot(U[:, :r + 1], sqrt(diag(Sigma[:r + 1])))

        return B

    def cov2corr(self, cov):
        """
        Compute correlation matrix from covariance

        Inputs:
        - cov: Covariance matrix
        """
        d = diag(cov)
        Corr = cov / sqrt(outer(d, d))

        return Corr

    # ----------- not used, kept for tests ---------

    def gbm_analytic(self, s0, mu, sdev, t, B):
        """
        Analytic solution
        Inputs:
        s0:
        mu:
        sigma:
        t:
        B:
        """

        drift = outer(mu - 0.5 * sdev ** 2, t)
        diffusion = dot(diag(sdev), B)
        S = dot(diag(s0), exp(drift + diffusion))

        return S
