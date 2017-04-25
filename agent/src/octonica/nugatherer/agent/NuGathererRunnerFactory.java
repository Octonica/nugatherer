package octonica.nugatherer.agent;

import jetbrains.buildServer.agent.AgentBuildRunnerInfo;
import jetbrains.buildServer.agent.BuildAgentConfiguration;
import jetbrains.buildServer.agent.runner.CommandLineBuildService;
import jetbrains.buildServer.agent.runner.CommandLineBuildServiceFactory;
import octonica.nugatherer.common.NuGathererProperties;
import org.jetbrains.annotations.NotNull;

public class NuGathererRunnerFactory implements CommandLineBuildServiceFactory {

  @NotNull
  public CommandLineBuildService createService() {
    return new NuGathererRunner();
  }

  @NotNull
  public AgentBuildRunnerInfo getBuildRunnerInfo() {
    return new AgentBuildRunnerInfo() {
      @NotNull
      public String getType() {
        return NuGathererProperties.RunnerType;
      }

      public boolean canRun(@NotNull BuildAgentConfiguration agentConfiguration) {
        return true;
      }
    };
  }
}
