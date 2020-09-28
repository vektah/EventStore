using System;
using System.Collections.Generic;

namespace EventStore.Core.Index {
	public static class IndexMapExtensions {
		public static MergeResult AddAndMergePTable(
			this IndexMap indexMap,
			PTable tableToAdd,
			int prepareCheckpoint,
			int commitCheckpoint,
			Func<string, ulong, ulong> upgradeHash,
			Func<IndexEntry, bool> existsAt,
			Func<IndexEntry, Tuple<string, bool>> recordExistsAt,
			IIndexFilenameProvider filenameProvider,
			byte version,
			int indexCacheDepth = 16,
			bool skipIndexVerify = false) {

			var addResult = indexMap.AddPTable(tableToAdd, prepareCheckpoint, commitCheckpoint);
			if (addResult.CanMergeAny) {
				var toDelete = new List<PTable>();
				MergeResult mergeResult;
				do {
					mergeResult = indexMap.TryMergeOneLevel(
						upgradeHash,
						existsAt,
						recordExistsAt,
						filenameProvider,
						version,
						indexCacheDepth,
						skipIndexVerify
					);

					toDelete.AddRange(mergeResult.ToDelete);
				} while (mergeResult.CanMergeAny);

				return new MergeResult(mergeResult.MergedMap, toDelete, true, false);
			}
			return new MergeResult(addResult.NewMap, new List<PTable>(), false, false);
		}
	}
}
