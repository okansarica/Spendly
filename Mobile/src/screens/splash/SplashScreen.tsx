import React, { useEffect, useRef } from 'react';
import {
    View,
    Text,
    Image,
    Animated,
    StyleSheet,
    ActivityIndicator,
    StatusBar,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import LinearGradient from 'react-native-linear-gradient';
import { useAppDispatch } from '../../store/hooks';
import { checkAuth } from '../../store/authStore';

export default function SplashScreen() {
    const dispatch = useAppDispatch();

    const scale = useRef(new Animated.Value(0.92)).current;
    const opacity = useRef(new Animated.Value(0)).current;
    const translateY = useRef(new Animated.Value(20)).current;

    useEffect(() => {
        Animated.parallel([
            Animated.timing(opacity, {
                toValue: 1,
                duration: 700,
                useNativeDriver: true,
            }),
            Animated.spring(scale, {
                toValue: 1,
                friction: 5,
                tension: 90,
                useNativeDriver: true,
            }),
            Animated.timing(translateY, {
                toValue: 0,
                duration: 700,
                useNativeDriver: true,
            }),
        ]).start(() => {
            Animated.loop(
                Animated.sequence([
                    Animated.timing(scale, { toValue: 1.025, duration: 1400, useNativeDriver: true }),
                    Animated.timing(scale, { toValue: 1, duration: 1400, useNativeDriver: true }),
                ])
            ).start();
        });

        const t = setTimeout(() => {
            dispatch(checkAuth());
        }, 10);

        return () => clearTimeout(t);
    }, [dispatch, opacity, scale, translateY]);

    return (
        <LinearGradient
            colors={[
                '#0B1220',
                '#111827',
                '#1E40AF',
            ]}
            locations={[0, 0.5, 1]}
            style={styles.gradient}
        >
            <StatusBar
                barStyle={'light-content'}
                translucent={false}
                backgroundColor={'#0B1220'}
            />

            <SafeAreaView style={styles.container}>
                <Animated.View
                    style={[
                        styles.brandContainer,
                        {
                            opacity,
                            transform: [{ scale }, { translateY }],
                        },
                    ]}
                >
                    <View style={styles.logoWrapper}>
                        <Image
                            source={require('../../assets/Spendly.jpg')}
                            style={styles.logo}
                            resizeMode="contain"
                        />
                    </View>

                    <Text style={[styles.title, { color: '#FFFFFF' }]}>Spendly</Text>

                    <Text style={[styles.subtitle, { color: 'rgba(255,255,255,0.7)' }]}>Smart expense tracking</Text>
                </Animated.View>

                <View style={styles.footer}>
                    <ActivityIndicator size="small" color="#FFFFFF" />
                    <Text style={styles.footerText}>Loading your finances...</Text>
                </View>
            </SafeAreaView>
        </LinearGradient>
    );
}

const styles = StyleSheet.create({
    gradient: {
        flex: 1,
    },
    container: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingVertical: 50,
    },
    brandContainer: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
    },
    logoWrapper: {
        width: 140,
        height: 140,
        borderRadius: 32,
        backgroundColor: 'rgba(255,255,255,0.06)',
        alignItems: 'center',
        justifyContent: 'center',
        marginBottom: 24,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 10 },
        shadowOpacity: 0.25,
        shadowRadius: 25,
        elevation: 10,
    },
    logo: {
        width: '70%',
        height: '70%',
    },
    title: {
        fontSize: 42,
        fontWeight: '800',
        letterSpacing: 0.5,
    },
    subtitle: {
        marginTop: 8,
        fontSize: 15,
        fontWeight: '500',
    },
    footer: {
        alignItems: 'center',
    },
    footerText: {
        marginTop: 8,
        fontSize: 12,
        color: 'rgba(255,255,255,0.6)',
    },
});
