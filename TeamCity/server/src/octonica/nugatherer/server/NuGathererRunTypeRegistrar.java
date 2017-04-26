package octonica.nugatherer.server;

import jetbrains.buildServer.serverSide.RunType;
import jetbrains.buildServer.serverSide.RunTypeRegistry;

import java.util.Collection;

public class NuGathererRunTypeRegistrar {

    public NuGathererRunTypeRegistrar(RunTypeRegistry registry, Collection<RunType> types){
        for (RunType type:types) {
            if(type==null)
                continue;
            registry.registerRunType(type);
        }
    }
}
