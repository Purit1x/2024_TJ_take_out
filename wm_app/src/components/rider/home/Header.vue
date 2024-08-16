<script setup>
    import { useRouter } from 'vue-router';
    import { useCookies } from '@vueuse/integrations/useCookies'  

    const router = useRouter()
    const cookies = useCookies(); // 使用 useCookies 钩子  

    onMounted(() => {  
        // 从 cookie 中读取用户信息  
        const riderData = cookies.get('rider');  
        cookies.set('rider', {});
        if (riderData) {  
        rider.value = riderData;  
        } else {  
        // 用户未登录，跳转到登录页面  
        router.push('/login');
        }  
    });  

    //退出
    const logout = () => {
        router.push("/login")
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
                    <span>{{ rider.value.name }}</span> <el-icon><Bell /></el-icon>
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