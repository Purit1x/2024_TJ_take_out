<script setup>
    import { useRouter } from 'vue-router';
    import { useStore } from "vuex" 
    import { ref,onMounted} from 'vue'
    import { stIdSearch } from "@/api/rider"
    import {getStationsInfo } from "@/api/platform"
    const router = useRouter()
    const store = useStore()    
    const rider = ref({})
    const stationId = ref(null); // 用于存储站点 ID  
    const riderStation = ref({}); // 用于存储站点信息  
    const ifStation = ref(true); // 用于判断是否有站点信息  
    onMounted(async () => {  
        // 从 cookie 中读取用户信息  
        const riderData = store.state.rider; 
        if (riderData) {  
            rider.value = riderData;  
        } else {   
            router.push('/login');
        }  
        stIdSearch(rider.value.RiderId).then(res => {  
            stationId.value = res.data; 
            getStationsInfo(stationId.value).then(res => {  
            riderStation.value = res.data; 
            }).catch(err => {  
                riderStation.value = "站点信息获取失败"; 
            });  
        }).catch(err => {  
            ifStation.value = false;  
        });   
    });  

    //退出
    const logout = () => {
        store.dispatch('clearRider'); 
        router.push('/login');
    }

</script>

<template>
    <div class="header">
        <div class="title">
            <div class="logo">
                <el-icon><MostlyCloudy /></el-icon>
            </div>
            <div class="text">
                <a href="/rider-home">外卖骑手网站</a>
            </div>
        </div>

        <div class="info">
            <div class="admin">
                <div class="name">
                    <span>&nbsp;欢迎，Id为{{ rider.RiderId }}的骑手!</span> <el-icon><Bell /></el-icon>
                </div>
                <div v-if="ifStation" class="name">
                    当前站点：{{ riderStation.stationName }}&nbsp;
                    地址：{{ riderStation.stationAddress }}&nbsp;
                </div>
                <div v-else class="name">
                    请等待站点分配&nbsp;
                </div>
                <div class="logout" @click="logout">
                    退出
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>

</style>