﻿using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SideCar.Models;

namespace SideCar.Services
{
	public class PackageService
	{
		private readonly BuildService _builds;
		private readonly IPackageStore _store;
		private readonly IPackageCompiler _compiler;
		private readonly ILogger<PackageService> _logger;

		public PackageService(BuildService builds, IPackageStore store, IPackageCompiler compiler, ILogger<PackageService> logger)
		{
			_builds = builds;
			_store = store;
			_compiler = compiler;
			_logger = logger;
		}

		public async Task<PackageResult> CompilePackageAsync(Assembly assembly, string buildHash, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var builds = await _builds.GetAvailableBuildsAsync(cancellationToken);
			if (!builds.Contains(buildHash))
				return new PackageResult { Successful = false, Errors = $"Build {buildHash} not available for packaging." };

			var provisioned = await _builds.TryProvisionBuildAsync(buildHash, cancellationToken);
			if (!provisioned)
				return new PackageResult { Successful = false, Errors = "Failed to provision build." };

			return await _compiler.CompilePackageAsync(assembly, buildHash);
		}

		public async Task<Assembly> FindPackageAssemblyByNameAsync(string packageName, CancellationToken cancellationToken = default)
		{
			return await _store.FindPackageAssemblyByNameAsync(packageName, cancellationToken);
		}

		public async Task<HashSet<string>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default)
		{
			return await _store.GetAvailablePackagesAsync(cancellationToken);
		}

		public async Task<string> LoadPackageContentAsync(string packageHash, PackageFile packageFile, CancellationToken cancellationToken = default)
		{
			return await _store.LoadPackageContentAsync(packageHash, packageFile, cancellationToken);
		}
	}
}
