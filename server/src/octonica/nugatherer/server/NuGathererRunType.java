package octonica.nugatherer.server;

import jetbrains.buildServer.serverSide.PropertiesProcessor;
import jetbrains.buildServer.serverSide.RunType;
import octonica.nugatherer.common.NuGathererProperties;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;

import java.util.HashMap;
import java.util.Map;

public class NuGathererRunType extends RunType {

    private final NuGathererPropertiesProcessor propertiesProcessor = new NuGathererPropertiesProcessor();
    private final HashMap<String,String> defaultProperties = new HashMap<String, String>();

    public NuGathererRunType(){
    }

    @NotNull
    public String getType() {
        return NuGathererProperties.RunnerType;
    }

    @NotNull
    public String getDisplayName() {
        return "NuGatherer";
    }

    @NotNull
    public String getDescription() {
        return "Gathers NuGets from a project file recursively";
    }

    @Nullable
    public PropertiesProcessor getRunnerPropertiesProcessor() {
        return propertiesProcessor;
    }

    @Nullable
    public String getEditRunnerParamsJspFilePath() {
        return "editNuGatherer.jsp";
    }

    @Nullable
    public String getViewRunnerParamsJspFilePath() {
        return "viewNuGatherer.jsp";
    }

    @Nullable
    public Map<String, String> getDefaultRunnerProperties() {
        return defaultProperties;
    }
}
