package octonica.nugatherer.agent;

import jetbrains.buildServer.RunBuildException;
import jetbrains.buildServer.agent.runner.ProgramCommandLine;
import org.jetbrains.annotations.NotNull;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class MsbuildCommandLine implements ProgramCommandLine {

  private final String workingDirectory;
  private final String executablePath;
  private final String rootProject;

  public MsbuildCommandLine(@NotNull String workingDirectory, @NotNull String executablePath, @NotNull String rootProject){

    this.workingDirectory = workingDirectory;
    this.executablePath = executablePath;
    this.rootProject = rootProject;
  }

  @NotNull
  @Override
  public String getExecutablePath() throws RunBuildException {
    return executablePath;
  }

  @NotNull
  @Override
  public String getWorkingDirectory() throws RunBuildException {
    return workingDirectory;
  }

  @NotNull
  @Override
  public List<String> getArguments() throws RunBuildException {
    return Arrays.asList("\"" + rootProject + "\"");
  }

  @NotNull
  @Override
  public Map<String, String> getEnvironment() throws RunBuildException {
    return new HashMap<String, String>();
  }
}
