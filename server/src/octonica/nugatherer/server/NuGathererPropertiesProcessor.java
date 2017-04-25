package octonica.nugatherer.server;

import jetbrains.buildServer.serverSide.InvalidProperty;
import jetbrains.buildServer.serverSide.PropertiesProcessor;
import octonica.nugatherer.common.NuGathererProperties;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Map;

public class NuGathererPropertiesProcessor implements PropertiesProcessor {

  public Collection<InvalidProperty> process(Map<String, String> properties) {
    if (properties == null) {
      return null;
    }
    List<InvalidProperty> invalidProperties = new ArrayList<InvalidProperty>();

    if (properties.containsKey(NuGathererProperties.MsbuildToolsVersionProperty)) {
      String value = properties.get(NuGathererProperties.MsbuildToolsVersionProperty);
      if (!NuGathererProperties.MsbuildToolsVersions.contains(value)) {
        invalidProperties.add(new InvalidProperty(NuGathererProperties.MsbuildToolsVersionProperty, "Unknown MSBuild Tools version."));
      }
    } else {
      invalidProperties.add(new InvalidProperty(NuGathererProperties.MsbuildToolsVersionProperty, "MSBuild Tools version must be specified."));
    }

    if (properties.containsKey(NuGathererProperties.MsbuildVersionProperty)) {
      String value = properties.get(NuGathererProperties.MsbuildVersionProperty);
      if (!NuGathererProperties.MsbuildVersions.contains(value)) {
        invalidProperties.add(new InvalidProperty(NuGathererProperties.MsbuildVersionProperty, "Unknown MSBuild version."));
      }
    } else {
      invalidProperties.add(new InvalidProperty(NuGathererProperties.MsbuildVersionProperty, "MSBuild version must be specified."));
    }

    String projectFile = null;
    if (properties.containsKey(NuGathererProperties.VirtualProjectRootProperty)) {
      projectFile = properties.get(NuGathererProperties.VirtualProjectRootProperty);
      if (projectFile != null) {
        projectFile = projectFile.trim();
      }
    }

    if (projectFile == null || projectFile.length() == 0) {
      invalidProperties.add(new InvalidProperty(NuGathererProperties.VirtualProjectRootProperty, "The path to the virtual project file must be specified."));
    }

    return invalidProperties;
  }
}
