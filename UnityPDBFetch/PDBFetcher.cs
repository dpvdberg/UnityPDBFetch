using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Deployment.Compression.Cab;

namespace UnityPDBFetch
{
    public class PDBFetcher
    {
        public static bool fetch(string name, string outdir)
        {
            if (outdir == null)
            {
                outdir = Path.GetDirectoryName(name);
            }
            
            var peHeader = new PeNet.PeFile(name);
            
            var pdbInfo = peHeader.ImageDebugDirectory[0].CvInfoPdb70;
            if (pdbInfo == null)
            {
                Console.WriteLine("Could not fetch pdb info from dll");
                return false;
            }
            var sigGuid = pdbInfo.Signature;
            var sig = sigGuid.ToString().Replace("-", "").ToUpper();

            var reg = new Regex(@"[^\\]*(?=[.][\w]+$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var matches = reg.Matches(pdbInfo.PdbFileName);

            if(matches.Count < 1)
            {
                Console.WriteLine("Could not parse signature from dll");
                return false;
            }

            var pdbName = matches[0];
            
            Console.WriteLine($"file '{name}' has debug signature '{sig}' and pdb name '{pdbName}'");

            string DownloadUrl =
                @"http://symbolserver.unity3d.com/" + pdbName + ".pdb/" + 
                sig + '1' + "/" + pdbName + ".pd_";
            // .pd_ is NOT a mistake, it is a .pdb compressed as a .cab
            // No idea why there is a 1 suffix for the signature

            // Unity files are compressed into .cabs.
            string outputCABFolder = Path.Combine(outdir, "cab");
            Directory.CreateDirectory(outputCABFolder);
            string outputCAB = Path.Combine(outputCABFolder, pdbName + ".cab");

            // Downloading cab
            if (!File.Exists(outputCAB))
            {
                Console.WriteLine($"Downloading from url: {DownloadUrl}");
                using (var client = new WebClient())
                {
                    client.DownloadFile(DownloadUrl, outputCAB);
                }
            }
            else
            {
                Console.WriteLine($"Cab file already exists: '{outputCAB}'");
            }

            if (!File.Exists(outputCAB))
            {
                Console.WriteLine("Could not fetch pdb from Unity symbol server");
                return false;
            }

            // Extracting the cab
            string outputPDB = Path.Combine(outdir, "pdb");
            Directory.CreateDirectory(outputPDB);

            CabInfo cab = new CabInfo(outputCAB);
            cab.Unpack(outputPDB);

            return true;
        }
    }
}