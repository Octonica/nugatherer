package octonica.nugatherer.common;

import org.jetbrains.annotations.Contract;
import org.jetbrains.annotations.Nullable;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class NuGathererProperties {

    public static final String RunnerType = "NuGatherer";

    public static final String MsbuildVersionProperty = "prop.msbuildVersion";
    public static final String MsbuildToolsVersionProperty = "prop.msbuildToolsVersion";
    public static final String VirtualProjectRootProperty = "prop.virtualProjectRoot";
    public static final String OutputFileProperty = "prop.outputFile";

    public static final List<String> MsbuildToolsVersions = Arrays.asList("15.0", "14.0", "12.0", "4.0", "none");

    private static final Map<String,String> MsbuildConfigurationMap = fillConfigurationMap();
    public static final List<String> MsbuildVersions = Arrays.asList("Microsoft Build Tools 2015", "Microsoft Build Tools 2013", "Microsoft .NET Framework 4.5", "Microsoft .NET Framework 4.0");

    public NuGathererProperties() {
    }

    private static Map<String, String> fillConfigurationMap(){
      HashMap<String, String > map = new HashMap<String, String>();
      map.put("Microsoft Build Tools 2015", "MSBuildTools14.0_x86_Path");
      map.put("Microsoft Build Tools 2013", "MSBuildTools12.0_x86_Path");
      map.put("Microsoft .NET Framework 4.5", "MSBuildTools3.5_x86_Path");
      map.put("Microsoft .NET Framework 4.0", "MSBuildTools4.0_x86_Path");
      return map;
    }

    public List<String> getMsbuildToolsVersions(){
        return MsbuildToolsVersions;
    }

    public List<String> getMsbuildVersions(){
        return MsbuildVersions;
    }

    @Contract("null -> null")
    @Nullable
    public static String getMsbuildConfigurationParameter(String selectedName){
        if(selectedName == null || !MsbuildConfigurationMap.containsKey(selectedName)){
            return null;
        }

        return MsbuildConfigurationMap.get(selectedName);
    }
}
